using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using chia.dotnet;
using chia.dotnet.console;

namespace rchia.Farm
{
    internal class FarmTasks : ConsoleTask
    {
        public FarmTasks(DaemonProxy daemon, IConsoleMessage consoleMessage)
            : base(consoleMessage)
        {
            Daemon = daemon;
        }

        public DaemonProxy Daemon { get; init; }

        public async Task Challenges(int limit)
        {
            using var cts = new CancellationTokenSource(1000);

            var farmer = new FarmerProxy(Daemon.RpcClient, Daemon.OriginService);
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
            var farmer = new FarmerProxy(Daemon.RpcClient, Daemon.OriginService);
            var fullNode = new FullNodeProxy(Daemon.RpcClient, Daemon.OriginService);
            var wallet = new WalletProxy(Daemon.RpcClient, Daemon.OriginService);

            using var cts = new CancellationTokenSource(2000);

            var all_harvesters = await farmer.GetHarvesters(cts.Token);
            var blockchain_state = await fullNode.GetBlockchainState(cts.Token);
            var farmer_running = await Daemon.IsServiceRunning(ServiceNames.Farmer, cts.Token);
            var full_node_running = await Daemon.IsServiceRunning(ServiceNames.FullNode, cts.Token);
            var wallet_running = await Daemon.IsServiceRunning(ServiceNames.Wallet, cts.Token);

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
        }
    }
}
