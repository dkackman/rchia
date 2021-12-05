using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Dynamic;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Farm;

internal sealed class SummaryCommand : EndpointOptions
{
    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving farm info...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(output, this);
            var daemon = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
            var farmer = daemon.CreateProxyFrom<FarmerProxy>();
            var fullNode = daemon.CreateProxyFrom<FullNodeProxy>();
            var wallet = daemon.CreateProxyFrom<WalletProxy>();

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var all_harvesters = await farmer.GetHarvesters(cts.Token);
            var blockchain_state = await fullNode.GetBlockchainState(cts.Token);
            var farmer_running = await daemon.IsServiceRunning(ServiceNames.Farmer, cts.Token);
            var full_node_running = await daemon.IsServiceRunning(ServiceNames.FullNode, cts.Token);
            var wallet_running = await daemon.IsServiceRunning(ServiceNames.Wallet, cts.Token);

            var status = blockchain_state is null
                ? "Not available"
                : blockchain_state.Sync.SyncMode
                ? "Syncing"
                : !blockchain_state.Sync.Synced
                ? "Not running"
                : !farmer_running
                ? "Not running"
                : "Farming";

            var summary = new Dictionary<string, string>
            {
                { "farming_status", status }
            };

            var wallet_ready = true;
            if (wallet_running)
            {
                try
                {
                    var (FarmedAmount, FarmerRewardAmount, FeeAmount, LastHeightFarmed, PoolRewardAmount) = await wallet.GetFarmedAmount(cts.Token);

                    summary.Add("total_chia_farmed", FarmedAmount.AsChia("F1"));
                    summary.Add("user_transaction_fees", FeeAmount.AsChia("F1"));
                    summary.Add("block_rewards", (FarmerRewardAmount + PoolRewardAmount).AsChia("F1"));
                    summary.Add("last_height_farmed", LastHeightFarmed.ToString("N0"));
                }
                catch
                {
                    wallet_ready = false;
                }
            }

            var harvesters = all_harvesters.ToList();

            var totalPlotCount = 0;
            var totalplotSize = (ulong)0;

            var localHarvesters = harvesters.Where(h => h.Connection.IsLocal);
            var remoteHarvesters = harvesters.Where(h => !h.Connection.IsLocal);

            if (localHarvesters.Any())
            {
                output.MarkupLine($"[wheat1]Local Harvester{(localHarvesters.Count() == 1 ? string.Empty : 's')}[/]");
                foreach (var harvester in localHarvesters)
                {
                    var size = harvester.Plots.Sum(p => (double)p.FileSize);
                    totalplotSize += (ulong)size;
                    totalPlotCount += harvester.Plots.Count;

                    output.MarkupLine($"  [green]{harvester.Connection.Host}[/]: [wheat1]{harvester.Plots.Count}[/] plots of size [wheat1]{size.ToBytesString("N1")}[/]");
                }
            }

            if (remoteHarvesters.Any())
            {
                output.MarkupLine($"[wheat1]Remote Harvester{(remoteHarvesters.Count() == 1 ? string.Empty : 's')}[/]");
                foreach (var harvester in remoteHarvesters)
                {
                    var size = harvester.Plots.Sum(p => (double)p.FileSize);
                    totalplotSize += (ulong)size;
                    totalPlotCount += harvester.Plots.Count;

                    output.MarkupLine($"  [green]{harvester.Connection.Host}:[/] [wheat1]{harvester.Plots.Count}[/] plots of size [wheat1]{size.ToBytesString("N1")}[/]");
                }
            }

            summary.Add("total_plot_count", totalPlotCount.ToString());
            summary.Add("total_plot_size", totalplotSize.ToBytesString("N1"));

            if (blockchain_state is not null)
            {
                summary.Add("estimated_network_space", blockchain_state.Space.ToBytesString());
            }
            else
            {
                summary.Add("estimated_network_space", "Unknown");
            }

            if (blockchain_state is not null && blockchain_state.Space != BigInteger.Zero)
            {
                if (totalPlotCount == 0)
                {
                    summary.Add("expected_time_to_win", "Never (no plots)");
                }
                else
                {
                    var proportion = totalplotSize / (double)blockchain_state.Space;
                    var blocktime = await fullNode.GetAverageBlockTime(cts.Token);
                    var span = blocktime / proportion;

                    summary.Add("expected_time_to_win", span.FormatTimeSpan());
                    output.Message($"Farming about {proportion:P6} percent of the network");
                }
            }

            if (Json)
            {
                dynamic result = new ExpandoObject();
                result.summary = summary.Sort();
                result.local_harvesters = localHarvesters;
                result.remote_harvesters = remoteHarvesters;
                output.WriteOutput(result);
            }
            else
            {
                output.WriteOutput(summary);
            }

            if (!wallet_running)
            {
                output.Helpful("For details on farmed rewards and fees you should run '[grey]rchia start wallet[/]' and '[grey]rchia wallet show[/]'", true);
            }
            else if (!wallet_ready)
            {
                output.Helpful("For details on farmed rewards and fees you should run '[grey]rchia wallet show[/]'", true);
            }
            else
            {
                output.Helpful("Note: log into your key using '[grey]rchia wallet show[/]' to see rewards for each key", true);
            }
        });
    }
}
