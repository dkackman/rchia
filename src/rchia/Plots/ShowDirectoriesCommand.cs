using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
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
            output.Helpful("Note that subdirectories must be added manually", true);
            output.MarkupLine("Add with '[grey]chia plots add <dir>[/]' and remove with '[grey]chia plots remove <dir>[/]'");
            output.WriteLine("");

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var list = new List<string>();
            foreach (var path in await proxy.GetPlotDirectories(cts.Token))
            {
                list.Add(path);
            }
            output.WriteOutput(list);
        });
    }
}
