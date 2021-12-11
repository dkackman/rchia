using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots;

internal sealed class ListPlotsCommand : EndpointOptions
{
    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving plot list...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Harvester);
            var proxy = new HarvesterProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var plots = await proxy.GetPlots(cts.Token);

            var result = new List<Dictionary<string, IEnumerable<string>>>()
            {
                new Dictionary<string, IEnumerable<string>>()
                {
                    { "failed_to_open", plots.FailedToOpenFilenames }
                },
                new Dictionary<string, IEnumerable<string>>()
                {
                    { "not_found", plots.NotFoundFileNames }
                },
                new Dictionary<string, IEnumerable<string>>()
                {
                    { "plots", plots.Plots.Select(p => p.Filename) }
                }
            };

            if (!Json)
            {
                ListPlots(output, plots.FailedToOpenFilenames, "failed to open");
                ListPlots(output, plots.NotFoundFileNames, "not found");
                ListPlots(output, plots.Plots.Select(p => p.Filename), "plots");
                if (!Verbose)
                {
                    output.Helpful("(use '[grey]-v/--verbose[/]' to see file names)", true);
                }
            }
            else
            {
                output.WriteOutput(result);
            }
        });
    }

    private void ListPlots(ICommandOutput output, IEnumerable<string> plots, string msg)
    {
        output.WriteLine($"{plots.Count()} {msg}.");
        if (plots.Any() && Verbose)
        {
            foreach (var plot in plots)
            {
                output.WriteLine(plot);
            }
        }
    }
}
