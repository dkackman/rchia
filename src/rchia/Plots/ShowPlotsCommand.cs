using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots
{
    internal sealed class ShowPlotsCommand : EndpointOptions
    {
        [Option("", "json", Description = "Set this flag to output json")]
        public bool Json { get; init; }

        [CommandTarget]
        public async Task<int> Run()
        {
            if (Json)
            {
                SetJsonOutput();
            }

            return await DoWorkAsync("Retrieving plot info...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Harvester);
                var proxy = new HarvesterProxy(rpcClient, ClientFactory.Factory.OriginService);

                WriteLine("Directories where plots are being searched for:");
                Helpful("Note that subdirectories must be added manually", true);
                MarkupLine("Add with '[grey]chia plots add <dir>[/]' and remove with '[grey]chia plots remove <dir>[/]'");
                WriteLine("");

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var output = new List<string>();
                foreach (var path in await proxy.GetPlotDirectories(cts.Token))
                {
                    output.Add(path);
                }
                WriteOutput(output);
            });
        }
    }
}
