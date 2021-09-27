using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots
{
    internal sealed class LogPlotsCommand : EndpointOptions
    {
        [Option("i", "id", Description = "The id of the plot log. Omit to see logs for all running plots.")]
        public string? Id { get; init; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await DoWorkAsync("Retrieving plot log...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(ctx, this, ServiceNames.Plotter);
                var proxy = new PlotterProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var q = await proxy.RegisterPlotter(cts.Token);

                if (string.IsNullOrEmpty(Id))
                {
                    var running = q.Where(p => p.PlotState == PlotState.RUNNING);
                    var count = running.Count();
                    MarkupLine($"There {(count == 1 ? "is" : "are")} [wheat1]{count}[/] running plot job{(count == 1 ? "" : "s")}");
                    foreach (var plot in running)
                    {
                        MarkupLine($"Log for plot [wheat1]{plot.Id}[/]");
                        WriteLine(plot.Log);
                        WriteLine("");
                    }
                }
                else
                {
                    var plot = q.FirstOrDefault(p => p.Id == Id);
                    if (plot is null)
                    {
                        throw new InvalidOperationException($"No plot with an id of {Id} was found");
                    }

                    WriteLine(plot.Log);
                    WriteLine("");
                }
            });
        }
    }
}
