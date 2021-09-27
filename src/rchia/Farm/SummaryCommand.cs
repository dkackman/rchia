using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Farm
{
    internal sealed class SummaryCommand : EndpointOptions
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            return await DoWorkAsync("Retrieving farm info...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(ctx, this, ServiceNames.Farmer);
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
                    ? "[red]Not available[/]"
                    : blockchain_state.Sync.SyncMode
                    ? "[grey]Syncing[/]"
                    : !blockchain_state.Sync.Synced
                    ? "[red]Not running[/]"
                    : !farmer_running
                    ? "[red]Not running[/]"
                    : "[green]Farming[/]";

                NameValue("Farming status", status);

                var wallet_ready = true;
                if (wallet_running)
                {
                    try
                    {
                        var (FarmedAmount, FarmerRewardAmount, FeeAmount, LastHeightFarmed, PoolRewardAmount) = await wallet.GetFarmedAmount(cts.Token);

                        NameValue("Total chia farmed", FarmedAmount.AsChia("F1"));
                        NameValue("User transaction fees", FeeAmount.AsChia("F1"));
                        NameValue("Block rewards", (FarmerRewardAmount + PoolRewardAmount).AsChia("F1"));
                        NameValue("Last height farmed", LastHeightFarmed);
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
                    MarkupLine($"[wheat1]Local Harvester{(localHarvesters.Count() == 1 ? string.Empty : 's')}[/]");
                    foreach (var harvester in localHarvesters)
                    {
                        var size = harvester.Plots.Sum(p => (double)p.FileSize);
                        totalplotSize += (ulong)size;
                        totalPlotCount += harvester.Plots.Count;

                        MarkupLine($"  [green]{harvester.Connection.Host}[/]: [wheat1]{harvester.Plots.Count}[/] plots of size [wheat1]{size.ToBytesString("N1")}[/]");
                    }
                }

                if (remoteHarvesters.Any())
                {
                    MarkupLine($"[wheat1]Remote Harvester{(remoteHarvesters.Count() == 1 ? string.Empty : 's')}[/]");
                    foreach (var harvester in remoteHarvesters)
                    {
                        var size = harvester.Plots.Sum(p => (double)p.FileSize);
                        totalplotSize += (ulong)size;
                        totalPlotCount += harvester.Plots.Count;

                        MarkupLine($"  [green]{harvester.Connection.Host}[/]: [wheat1]{harvester.Plots.Count}[/] plots of size [wheat1]{size.ToBytesString("N1")}[/]");
                    }
                }

                NameValue("Plot count for all harvesters", totalPlotCount);
                NameValue("Total size of plots", totalplotSize.ToBytesString("N1"));

                if (blockchain_state is not null)
                {
                    NameValue("Estimated network space", blockchain_state.Space.ToBytesString());
                }
                else
                {
                    NameValue("Estimated network space", "Unknown");
                }

                if (blockchain_state is not null && blockchain_state.Space != BigInteger.Zero)
                {
                    if (totalPlotCount == 0)
                    {
                        NameValue("Expected time to win", "Never (no plots)");
                    }
                    else
                    {
                        var proportion = totalplotSize / (double)blockchain_state.Space;
                        var blocktime = await fullNode.GetAverageBlockTime(cts.Token);
                        var span = blocktime / proportion;

                        NameValue("Expected time to win", span.FormatTimeSpan());
                        Message($"Farming about {proportion:P6} percent of the network");
                    }
                }

                if (!wallet_running)
                {
                    Helpful("For details on farmed rewards and fees you should run '[grey]rchia start wallet[/]' and '[grey]rchia wallet show[/]'", true);
                }
                else if (!wallet_ready)
                {
                    Helpful("For details on farmed rewards and fees you should run '[grey]rchia wallet show[/]'", true);
                }
                else
                {
                    Helpful("Note: log into your key using '[grey]rchia wallet show[/]' to see rewards for each key", true);
                }
            });
        }
    }
}
