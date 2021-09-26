using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots
{
    internal sealed class ShowPlotsCommand : EndpointOptions
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            return await DoWork2("Retrieving plot info...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Harvester);
                var proxy = new HarvesterProxy(rpcClient, ClientFactory.Factory.OriginService);

                WriteLine("Directories where plots are being searched for:");
                Helpful("Note that subdirectories must be added manually", true);
                MarkupLine("Add with '[grey]chia plots add <dir>[/]' and remove with '[grey]chia plots remove <dir>[/]'");
                WriteLine("");

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                foreach (var path in await proxy.GetPlotDirectories(cts.Token))
                {
                    WriteLine($"{path}");
                }
            });
        }
    }
}
