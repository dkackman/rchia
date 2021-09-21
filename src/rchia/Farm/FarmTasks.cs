using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using Spectre.Console;

namespace rchia.Farm
{
    internal class FarmTasks : ConsoleTask<DaemonProxy>
    {
        public FarmTasks(DaemonProxy daemon, IConsoleMessage consoleMessage, int timeoutSeconds)
            : base(daemon, consoleMessage, timeoutSeconds)
        {
        }

        public async Task Challenges(int limit)
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            var farmer = new FarmerProxy(Service.RpcClient, Service.OriginService);
            var signagePoints = await farmer.GetSignagePoints(cts.Token);

            var list = signagePoints.Reverse().ToList(); // convert to list to avoid multiple iteration
            var count = limit == 0 ? list.Count : limit;

            var table = new Table();
            table.AddColumn("[orange3]Index[/]");
            table.AddColumn("[orange3]Hash[/]");

            foreach (var sp in list.Take(count))
            {
                table.AddRow(sp.SignagePoint.SignagePointIndex.ToString(), sp.SignagePoint.ChallengeHash);
            }

            AnsiConsole.Render(table);
            ConsoleMessage.Message($"Showing {count} of {list.Count} challenges.");
        }

        public async Task Summary()
        {
            var farmer = new FarmerProxy(Service.RpcClient, Service.OriginService);
            var fullNode = new FullNodeProxy(Service.RpcClient, Service.OriginService);
            var wallet = new WalletProxy(Service.RpcClient, Service.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

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
                ConsoleMessage.MarkupLine($"[wheat1]Local Harvester{(localHarvesters.Count() > 1 ? 's' : ' ')}[/]");
                foreach (var harvester in localHarvesters)
                {
                    var size = harvester.Plots.Sum(p => (double)p.FileSize);
                    totalplotSize += (ulong)size;
                    totalPlotCount += harvester.Plots.Count;

                    ConsoleMessage.MarkupLine($"  [green]{harvester.Connection.Host}[/]: [wheat1]{harvester.Plots.Count}[/] plots of size [wheat1]{size.ToBytesString("N1")}[/]");
                }
            }

            if (remoteHarvesters.Any())
            {
                ConsoleMessage.MarkupLine($"[wheat1]Remote Harvester{(remoteHarvesters.Count() > 1 ? 's' : ' ')}[/]");
                foreach (var harvester in remoteHarvesters)
                {
                    var size = harvester.Plots.Sum(p => (double)p.FileSize);
                    totalplotSize += (ulong)size;
                    totalPlotCount += harvester.Plots.Count;

                    ConsoleMessage.MarkupLine($"  [green]{harvester.Connection.Host}[/]: [wheat1]{harvester.Plots.Count}[/] plots of size [wheat1]{size.ToBytesString("N1")}[/]");
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
                ConsoleMessage.Helpful("For details on farmed rewards and fees you should run [grey]rchia start wallet[/] and [grey]rchia wallet show[/]", true);
            }
            else if (!wallet_ready)
            {
                ConsoleMessage.Helpful("For details on farmed rewards and fees you should run [grey]rchia wallet show[/]", true);
            }
            else
            {
                ConsoleMessage.Helpful("Note: log into your key using [grey]rchia wallet show[/] to see rewards for each key", true);
            }
        }
    }
}
