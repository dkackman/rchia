using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots
{
    internal sealed class ListPlotsCommand : EndpointOptions
    {
        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Retrieving plot list...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Harvester);
                var proxy = new HarvesterProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var plots = await proxy.GetPlots(cts.Token);

                ListPlots(plots.FailedToOpenFilenames, "failed to open");
                ListPlots(plots.NotFoundFileNames, "not found");
                ListPlots(plots.Plots.Select(p => p.Filename), "plots");

                if (!Verbose)
                {
                    Helpful("(use '[grey]-v/--verbose[/]' to see file names)", true);
                }
            });
        }

        private void ListPlots(IEnumerable<string> plots, string msg)
        {
            WriteLine($"{plots.Count()} {msg}.");
            if (plots.Any() && Verbose)
            {
                foreach (var plot in plots)
                {
                    WriteLine(plot);
                }
            }
        }

    }
}
