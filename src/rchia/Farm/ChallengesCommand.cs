using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using Spectre.Console;
using System.ComponentModel;
using Spectre.Console.Cli;

namespace rchia.Farm
{
    [Description("Show the latest challenges")]
    internal sealed class ChallengesCommand : AsyncCommand<ChallengesCommand.ChallengesSettings>
    {
        public sealed class ChallengesSettings : EndpointSettings
        {
            [Description("Limit the number of challenges shown. Use 0 to disable the limit")]
            [CommandArgument(0, "[limit]")]
            [DefaultValue(20)]
            public int Limit { get; set; } = 20;
        }

        public async override Task<int> ExecuteAsync(CommandContext context, ChallengesSettings settings)
        {
            var worker = new Worker()
            {
                Verbose = settings.Verbose
            };

            return await worker.DoWorkAsync("Retrieving challenges...", async ctx =>
            {
                using var rpcClient = await ClientFactory2.Factory.CreateRpcClient(ctx, settings, ServiceNames.Farmer);
                var farmer = new FarmerProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(settings.TimeoutMilliseconds);
                var signagePoints = await farmer.GetSignagePoints(cts.Token);

                var list = signagePoints.Reverse().ToList(); // convert to list to avoid multiple iteration
                var count = settings.Limit == 0 ? list.Count : settings.Limit;

                var table = new Table
                {
                    Title = new TableTitle("Challenges")
                };
                table.AddColumn("[orange3]Index[/]");
                table.AddColumn("[orange3]Hash[/]");

                foreach (var sp in list.Take(count))
                {
                    table.AddRow(sp.SignagePoint.SignagePointIndex.ToString(), sp.SignagePoint.ChallengeHash);
                }

                AnsiConsole.Write(table);
                worker.Message($"Showing {count} of {list.Count} challenges.");
            });
        }
    }
}
