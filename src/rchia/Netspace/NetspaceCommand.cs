using System;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Netspace
{
    [Command("netspace", Description = "Calculates the estimated space on the network given two block header hashes.\nRequires a daemon or full_node endpoint.")]
    internal sealed class NetspaceCommand : EndpointOptions
    {
        [Option("d", "delta-block-height", Default = 4608, ArgumentHelpName = "DELTA", Description = "Compare a block X blocks older to estimate total network space. Defaults to 4608 blocks" +
                                                                                         "(~1 day) and Peak block as the starting block. Use --start HEADER_HASH to specify" +
                                                                                         "starting block. Use 192 blocks to estimate over the last hour.")]
        public uint DeltaBlockHeight { get; init; } = 4608;

        [Option("s", "start", ArgumentHelpName = "HEADER_HASH", Description = "Newest block used to calculate estimated total network space.Defaults to Peak block.")]
        public string? Start { get; init; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await DoWorkAsync("Retrieving network info...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.FullNode);

                var proxy = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                uint newer_block_height = 0;
                if (string.IsNullOrEmpty(Start))
                {
                    var blockchain_state = await proxy.GetBlockchainState(cts.Token);
                    if (blockchain_state.Peak is null)
                    {
                        Warning("No blocks in blockchain");
                        return;
                    }

                    newer_block_height = blockchain_state.Peak.Height;
                }
                else
                {
                    var newer_block = await proxy.GetBlockRecord(Start, cts.Token);
                    if (newer_block is null)
                    {
                        Warning($"Block header hash {Start} not found.");
                        return;
                    }

                    newer_block_height = newer_block.Height;
                }

                var newer_block_header = await proxy.GetBlockRecordByHeight(newer_block_height);
                var older_block_height = Math.Max(0, newer_block_height - DeltaBlockHeight);
                var older_block_header = await proxy.GetBlockRecordByHeight(older_block_height);
                var network_space_bytes_estimate = await proxy.GetNetworkSpace(newer_block_header.HeaderHash, older_block_header.HeaderHash);

                if (Verbose)
                {
                    MarkupLine("[wheat1]Older Block[/]");
                    NameValue("  Block Height", older_block_header.Height);
                    NameValue("  Weight", older_block_header.Weight);
                    NameValue("  VDF Iterations", older_block_header.TotalIters.ToString("N0"));
                    NameValue("  Header Hash", $"0x{ older_block_header.HeaderHash}");

                    MarkupLine("[wheat1]Newer Block[/]");
                    NameValue("  Block Height", newer_block_header.Height);
                    NameValue("  Weight", newer_block_header.Weight);
                    NameValue("  VDF Iterations", newer_block_header.TotalIters.ToString("N0"));
                    NameValue("  Header Hash", $"0x{ newer_block_header.HeaderHash}");
                }

                WriteLine(network_space_bytes_estimate.ToBytesString());
            });
        }
    }
}
