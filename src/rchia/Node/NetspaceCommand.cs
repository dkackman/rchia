using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Node;

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
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving network info...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.FullNode);
            var proxy = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            uint newer_block_height = 0;
            if (string.IsNullOrEmpty(Start))
            {
                var blockchain_state = await proxy.GetBlockchainState(cts.Token);
                if (blockchain_state.Peak is null)
                {
                    output.WriteWarning("No blocks in blockchain");
                    return;
                }

                newer_block_height = blockchain_state.Peak.Height;
            }
            else
            {
                var newer_block = await proxy.GetBlockRecord(Start, cts.Token);
                if (newer_block is null)
                {
                    output.WriteWarning($"Block header hash {Start} not found.");
                    return;
                }

                newer_block_height = newer_block.Height;
            }

            var newer_block_header = await proxy.GetBlockRecordByHeight(newer_block_height);
            var older_block_height = Math.Max(0, newer_block_height - DeltaBlockHeight);
            var older_block_header = await proxy.GetBlockRecordByHeight(older_block_height);
            var network_space_estimate = await proxy.GetNetworkSpace(newer_block_header.HeaderHash, older_block_header.HeaderHash);

            output.WriteOutput("network_space_estimate", new Formattable<BigInteger>(network_space_estimate, space => space.ToBytesString()), Verbose);
        });
    }
}
