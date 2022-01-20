using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using Spectre.Console;

namespace rchia.Node;

internal sealed class StatusCommand : EndpointOptions
{
    [Option("a", "about", IsHidden = true)]
    public bool About { get; init; }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving node info...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.FullNode);
            var proxy = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var state = await proxy.GetBlockchainState(cts.Token);
            var (NetworkName, NetworkPrefix) = await proxy.GetNetworkInfo(cts.Token);

            var peakHash = state.Peak is not null ? state.Peak.HeaderHash : string.Empty;

            var result = new Dictionary<string, object?>()
            {
                { "network", NetworkName }
            };

            if (state.Sync.Synced)
            {
                result.Add("blockchain_status", new Formattable<string>("Full Node Synced", "green"));
                result.Add("peak_hash", new Formattable<string>(peakHash, peakHash => peakHash.Replace("0x", string.Empty)));
            }
            else if (state.Peak is not null && state.Sync.SyncMode)
            {
                result.Add("blockchain_status", new Formattable<string>("Syncing", "orange3"));
                result.Add("sync_height", $"{state.Sync.SyncProgressHeight:N0} of {state.Sync.SyncTipHeight:N0}");
                result.Add("blocks_behind", state.Sync.SyncTipHeight - state.Sync.SyncProgressHeight);
                result.Add("peak_hash", new Formattable<string>(peakHash, peakHash => peakHash.Replace("0x", string.Empty)));
            }
            else if (state.Peak is not null)
            {
                result.Add("blockchain_status", new Formattable<string>("Not Synced", "red"));
            }
            else
            {
                output.WriteWarning("The node is searching for an initial chain");
                output.WriteMarkupLine("You may be able to expedite with '[grey]rchia show add host:port[/]' using a known node.");
            }

            if (state.Peak is not null)
            {
                var peak_time = state.Peak.DateTimestamp;
                if (!state.Peak.IsTransactionBlock)
                {
                    var curr = await proxy.GetBlockRecord(state.Peak.HeaderHash, cts.Token);

                    while (curr is not null && !curr.IsTransactionBlock)
                    {
                        curr = await proxy.GetBlockRecord(curr.PrevHash, cts.Token);
                    }

                    peak_time = curr?.DateTimestamp;
                }

                var time = peak_time.HasValue ? peak_time.Value.ToLocalTime().ToString("F") : "unknown";
                result.Add("peak_time", time);
                result.Add("peak_height", state.Peak.Height);
            }

            result.Add("estimated_network_space", new Formattable<BigInteger>(state.Space, space => space.ToBytesString()));
            result.Add("current_difficulty", state.Difficulty);
            result.Add("current_vdf_sub_slot_iters", state.SubSlotIters);

            var totalIters = state.Peak is not null ? state.Peak.TotalIters : 0;
            result.Add("total_iterations_since_start", totalIters);

            output.WriteOutput(result);

            if (!Json && Verbose && About)
            {
                AnsiConsole.Write(new FigletText("CHIA!")
                                    .Centered()
                                    .Color(Color.Green));
            }
        });
    }
}
