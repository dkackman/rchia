using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots;

internal sealed class ShowPlotLogCommand : EndpointOptions
{
    [Option("i", "id", Description = "The id of the plot log. Omit to see logs for all running plots.")]
    public string? Id { get; init; }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving plot log...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(output, this);
            var proxy = new PlotterProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var q = await proxy.RegisterPlotter(cts.Token);

            if (string.IsNullOrEmpty(Id))
            {
                if (Json)
                {
                    output.WriteOutput(q);
                }
                else
                {
                    var running = q.Where(p => p.PlotState == PlotState.RUNNING);
                    var count = running.Count();
                    output.MarkupLine($"There {(count == 1 ? "is" : "are")} [wheat1]{count}[/] running plot job{(count == 1 ? "" : "s")}");
                    foreach (var plot in running)
                    {
                        output.MarkupLine($"Log for plot [wheat1]{plot.Id}[/]");
                        output.WriteLine(plot.Log);
                        output.WriteLine("");
                    }
                }
            }
            else
            {
                var plot = q.FirstOrDefault(p => p.Id == Id);
                if (plot is null)
                {
                    throw new InvalidOperationException($"No plot with an id of {Id} was found");
                }

                output.WriteOutput("log", plot.Log ?? plot.State, Verbose);
            }
        });
    }
}
