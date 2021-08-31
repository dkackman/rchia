using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Numerics;
using System.Text;

using chia.dotnet;
using chia.dotnet.console;

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
            using var cts = new CancellationTokenSource(1000);

            var farmer = new FarmerProxy(Service.RpcClient, Service.OriginService);
            var signagePoints = await farmer.GetSignagePoints(cts.Token);

            var list = signagePoints.Reverse().ToList(); // convert to list to avoid multiple iteration
            var count = limit == 0 ? list.Count : limit;
            foreach (var sp in list.Take(count))
            {
                Console.WriteLine($"Hash: {sp.SignagePoint.ChallengeHash} Index: {sp.SignagePoint.SignagePointIndex}");
            }

            ConsoleMessage.Message($"Showing {count} of {list.Count} challenges.");
        }

        public async Task Summary()
        {
            var farmer = new FarmerProxy(Service.RpcClient, Service.OriginService);
            var fullNode = new FullNodeProxy(Service.RpcClient, Service.OriginService);
            var wallet = new WalletProxy(Service.RpcClient, Service.OriginService);

            using var cts = new CancellationTokenSource(2000);

            var all_harvesters = await farmer.GetHarvesters(cts.Token);
            var blockchain_state = await fullNode.GetBlockchainState(cts.Token);
            var farmer_running = await Service.IsServiceRunning(ServiceNames.Farmer, cts.Token);
            var full_node_running = await Service.IsServiceRunning(ServiceNames.FullNode, cts.Token);
            var wallet_running = await Service.IsServiceRunning(ServiceNames.Wallet, cts.Token);

            var status = blockchain_state is null
                ? "Not available"
                : blockchain_state.Sync.SyncMode
                ? "Syncing"
                : !blockchain_state.Sync.Synced
                ? "Not running"
                : !farmer_running
                ? "Not running"
                : "Farming";

            Console.WriteLine($"Farming status: {status}");

            var wallet_ready = true;
            if (wallet_running)
            {
                try
                {
                    var (FarmedAmount, FarmerRewardAmount, FeeAmount, LastHeightFarmed, PoolRewardAmount) = await wallet.GetFarmedAmount(cts.Token);

                    Console.WriteLine($"Total chia farmed: {FarmedAmount.AsChia("F1")}");
                    Console.WriteLine($"User transaction fees: {FeeAmount.AsChia("F1")}");
                    Console.WriteLine($"Block rewards: {(FarmerRewardAmount + PoolRewardAmount).AsChia("F1")}");
                    Console.WriteLine($"Last height farmed: {LastHeightFarmed}");
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
                Console.WriteLine($"Local Harvester{(localHarvesters.Count() > 1 ? 's' : ' ')}");
                foreach (var harvester in localHarvesters)
                {
                    var size = harvester.Plots.Sum(p => (double)p.FileSize);
                    totalplotSize += (ulong)size;
                    totalPlotCount += harvester.Plots.Count;

                    Console.WriteLine($"  {harvester.Connection.Host}: {harvester.Plots.Count} plots of size {size.ToBytesString("N1")}");
                }
            }

            if (remoteHarvesters.Any())
            {
                Console.WriteLine($"Remote Harvester{(remoteHarvesters.Count() > 1 ? 's' : ' ')}");
                foreach (var harvester in remoteHarvesters)
                {
                    var size = harvester.Plots.Sum(p => (double)p.FileSize);
                    totalplotSize += (ulong)size;
                    totalPlotCount += harvester.Plots.Count;

                    Console.WriteLine($"  {harvester.Connection.Host}: {harvester.Plots.Count} plots of size {size.ToBytesString("N1")}");
                }
            }

            Console.WriteLine($"Plot count for all harvesters: {totalPlotCount}");
            Console.WriteLine($"Total size of plots: {totalplotSize.ToBytesString("N1")}");

            if (blockchain_state is not null)
            {
                Console.WriteLine($"Estimated network space: {blockchain_state.Space}");
            }
            else
            {
                Console.WriteLine("Estimated network space: Unknown");
            }

            if (blockchain_state is not null && blockchain_state.Space != BigInteger.Zero)
            {
                if (totalPlotCount == 0)
                {
                    Console.WriteLine("Expected time to win: Never (no plots)");
                }
                else
                {
                    var proportion = totalplotSize / (double)blockchain_state.Space;
                    var blocktime = await fullNode.GetAverageBlockTime(cts.Token);
                    var span = blocktime / proportion;

                    Console.WriteLine($"Expected time to win: {span.FormatTimeSpan()}");
                    ConsoleMessage.Message($"Farming about {proportion:P} percent of the network");
                }
            }

            if (!wallet_running)
            {
                Console.WriteLine("For details on farmed rewards and fees you should run 'chia start wallet' and 'chia wallet show'");
            }
            else if (!wallet_ready)
            {
                Console.WriteLine("For details on farmed rewards and fees you should run 'chia wallet show'");
            }
            else
            {
                Console.WriteLine("Note: log into your key using 'chia wallet show' to see rewards for each key");
            }
        }
    }

    public static class Extensions
    {
        public static string FormatTimeSpan(this TimeSpan t)
        {
            var builder = new StringBuilder();
            if (t.Days > 0)
            {
                _ = builder.Append($"{t.Days} day{(t.Days > 1 ? "s" : "")} ");
            }

            if (t.Hours > 0)
            {
                _ = builder.Append($"{t.Hours} hour{(t.Hours > 1 ? "s" : "")} ");
            }

            if (t.Minutes > 0)
            {
                _ = builder.Append($"{t.Minutes} minute{(t.Minutes > 1 ? "s" : "")} ");
            }

            return builder.ToString();
        }
    }
}
