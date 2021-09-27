using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using Spectre.Console;

namespace rchia.Farm
{
    internal sealed class ChallengesCommand : EndpointOptions
    {
        [Argument(0, Name = "limit", Default = 20, Description = "Limit the number of challenges shown. Use 0 to disable the limit")]
        public int Limit { get; init; } = 20;

        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Retrieving challenges...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Farmer);
                var farmer = new FarmerProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var signagePoints = await farmer.GetSignagePoints(cts.Token);

                var list = signagePoints.Reverse().ToList(); // convert to list to avoid multiple iteration
                var count = Limit == 0 ? list.Count : Limit;

                var table = new Table();
                table.AddColumn("[orange3]Index[/]");
                table.AddColumn("[orange3]Hash[/]");

                foreach (var sp in list.Take(count))
                {
                    table.AddRow(sp.SignagePoint.SignagePointIndex.ToString(), sp.SignagePoint.ChallengeHash);
                }

                AnsiConsole.Render(table);
                Message($"Showing {count} of {list.Count} challenges.");
            });
        }
    }
}
