using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots;

internal sealed class ShowDirectoriesCommand : EndpointOptions
{
    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving plot info...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Harvester);
            var proxy = new HarvesterProxy(rpcClient, ClientFactory.Factory.OriginService);

            output.WriteLine("Directories where plots are being searched for:");
            output.WriteMessage("Note that subdirectories must be added manually", false);
            output.WriteMarkupLine("Add with '[grey]chia plots add <dir>[/]' and remove with '[grey]chia plots remove <dir>[/]'");

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var dirs = from path in await proxy.GetPlotDirectories(cts.Token)
                       select path;
            output.WriteOutput(dirs);
        });
    }
}
