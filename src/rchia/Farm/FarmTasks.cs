using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Farm
{
    internal class FarmTasks : ConsoleTask<DaemonProxy>
    {
        public FarmTasks(DaemonProxy daemon, IConsoleMessage consoleMessage)
            : base(daemon, consoleMessage)
        {
        }

        public async Task Challenges(int limit)
        {
            using var cts = new CancellationTokenSource(30000);

            var farmer = new FarmerProxy(Service.RpcClient, Service.OriginService);
            var signagePoints = await farmer.GetSignagePoints(cts.Token);

            var list = signagePoints.Reverse().ToList(); // convert to list to avoid multiple iteration
            var count = limit == 0 ? list.Count : limit;
            foreach (var sp in list.Take(count))
            {
                ConsoleMessage.NameValue("Hash", sp.SignagePoint.ChallengeHash);
                ConsoleMessage.NameValue("Index", sp.SignagePoint.SignagePointIndex);
            }

            ConsoleMessage.Message($"Showing {count} of {list.Count} challenges.");
        }

        public async Task Summary()
        {
            var farmer = new FarmerProxy(Service.RpcClient, Service.OriginService);
            var fullNode = new FullNodeProxy(Service.RpcClient, Service.OriginService);
            var wallet = new WalletProxy(Service.RpcClient, Service.OriginService);

            using var cts = new CancellationTokenSource(30000);

            var all_harvesters = await farmer.GetHarvesters(cts.Token);
            var blockchain_state = await fullNode.GetBlockchainState(cts.Token);
            var farmer_running = await Service.IsServiceRunning(ServiceNames.Farmer, cts.Token);
            var full_node_running = await Service.IsServiceRunning(ServiceNames.FullNode, cts.Token);
            var wallet_running = await Service.IsServiceRunning(ServiceNames.Wallet, cts.Token);

            var status = blockchain_state is null
                ? "[red]Not available[/]"
                : blockchain_state.Sync.SyncMode
                ? "[grey]Syncing[/]"
                : !blockchain_state.Sync.Synced
                ? "[red]Not running[/]"
                : !farmer_running
                ? "[red]Not running[/]"
                : "[green]Farming[/]";

            ConsoleMessage.NameValue("Farming status", status);

            var wallet_ready = true;
            if (wallet_running)
            {
                try
                {
                    var (FarmedAmount, FarmerRewardAmount, FeeAmount, LastHeightFarmed, PoolRewardAmount) = await wallet.GetFarmedAmount(cts.Token);

                    ConsoleMessage.NameValue("Total chia farmed", FarmedAmount.AsChia("F1"));
                    ConsoleMessage.NameValue("User transaction fees", FeeAmount.AsChia("F1"));
                    ConsoleMessage.NameValue("Block rewards", (FarmerRewardAmount + PoolRewardAmount).AsChia("F1"));
                    ConsoleMessage.NameValue("Last height farmed", LastHeightFarmed);
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
                ConsoleMessage.MarkupLine($"[bold]Local Harvester{(localHarvesters.Count() > 1 ? 's' : ' ')}[/]");
                foreach (var harvester in localHarvesters)
                {
                    var size = harvester.Plots.Sum(p => (double)p.FileSize);
                    totalplotSize += (ulong)size;
                    totalPlotCount += harvester.Plots.Count;

                    ConsoleMessage.MarkupLine($"  [green]{harvester.Connection.Host}[/]: [bold]{harvester.Plots.Count}[/] plots of size [bold]{size.ToBytesString("N1")}[/]");
                }
            }

            if (remoteHarvesters.Any())
            {
                ConsoleMessage.MarkupLine($"[bold]Remote Harvester{(remoteHarvesters.Count() > 1 ? 's' : ' ')}[/]");
                foreach (var harvester in remoteHarvesters)
                {
                    var size = harvester.Plots.Sum(p => (double)p.FileSize);
                    totalplotSize += (ulong)size;
                    totalPlotCount += harvester.Plots.Count;

                    ConsoleMessage.MarkupLine($"  [green]{harvester.Connection.Host}[/]: [bold]{harvester.Plots.Count}[/] plots of size [bold]{size.ToBytesString("N1")}[/]");
                }
            }

            ConsoleMessage.NameValue("Plot count for all harvesters", totalPlotCount);
            ConsoleMessage.NameValue("Total size of plots", totalplotSize.ToBytesString("N1"));

            if (blockchain_state is not null)
            {
                ConsoleMessage.NameValue("Estimated network space", blockchain_state.Space.ToBytesString());
            }
            else
            {
                ConsoleMessage.NameValue("Estimated network space", "Unknown");
            }

            if (blockchain_state is not null && blockchain_state.Space != BigInteger.Zero)
            {
                if (totalPlotCount == 0)
                {
                    ConsoleMessage.NameValue("Expected time to win", "Never (no plots)");
                }
                else
                {
                    var proportion = totalplotSize / (double)blockchain_state.Space;
                    var blocktime = await fullNode.GetAverageBlockTime(cts.Token);
                    var span = blocktime / proportion;

                    ConsoleMessage.NameValue("Expected time to win", span.FormatTimeSpan());
                    ConsoleMessage.Message($"Farming about {proportion:P} percent of the network");
                }
            }

            if (!wallet_running)
            {
                ConsoleMessage.WriteLine("For details on farmed rewards and fees you should run 'chia start wallet' and 'chia wallet show'");
            }
            else if (!wallet_ready)
            {
                ConsoleMessage.WriteLine("For details on farmed rewards and fees you should run 'chia wallet show'");
            }
            else
            {
                ConsoleMessage.WriteLine("Note: log into your key using 'chia wallet show' to see rewards for each key");
            }
        }
    }
}
