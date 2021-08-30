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
        public FarmTasks(FarmerProxy farmer, IConsoleMessage consoleMessage)
            : base(consoleMessage)
        {
            Farmer = farmer;
        }

        public FarmerProxy Farmer { get; init; }

        public async Task Challenges(int limit)
        {
            using var cts = new CancellationTokenSource(1000);
            var signagePoints = await Farmer.GetSignagePoints(cts.Token);

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

        }
    }
}
