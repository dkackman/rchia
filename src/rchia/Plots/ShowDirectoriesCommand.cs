using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots;

internal sealed class ShowDirectoriesCommand : EndpointOptions
{
    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving directory info...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Harvester);
            var proxy = new HarvesterProxy(rpcClient, ClientFactory.Factory.OriginService);

            output.WriteLine("Directories where plots are being searched for:");
            output.WriteMessage("Note that subdirectories must be added manually", false);
            output.WriteMarkupLine("Add with '[grey]rchia plots add <dir>[/]' and remove with '[grey]rchia plots remove <dir>[/]'");

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var dirs = from path in await proxy.GetPlotDirectories(cts.Token)
                       select path;
            output.WriteOutput(dirs);
        });
    }
}
