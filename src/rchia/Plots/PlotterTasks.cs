using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots
{
    internal class PlotterTasks : ConsoleTask<PlotterProxy>
    {
        public PlotterTasks(PlotterProxy proxy, IConsoleMessage consoleMessage, int timeoutSeconds)
            : base(proxy, consoleMessage, timeoutSeconds)
        {
        }

        public async Task CreatePlots(PlotterConfig config)
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            var q = await Service.RegisterPlotter(cts.Token);
            await Service.StartPlotting(config, cts.Token);

            ConsoleMessage.Helpful("Plot queued. Run [grey]rchia plots queue -v[/] or [grey]rchia plots log[/] to check status", true);
        }

        public async Task Queue()
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            var q = await Service.RegisterPlotter(cts.Token);
            var plots = from p in q
                        group p by p.PlotState into g
                        select g;

            foreach (var group in plots)
            {
                ConsoleMessage.NameValue(group.Key.ToString(), group.Count());
                if (ConsoleMessage.Verbose)
                {
                    foreach (var item in group)
                    {
                        ConsoleMessage.WriteLine($"  {item.Id}");
                    }
                }
            }
        }

        public async Task Log(string? plotId)
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var q = await Service.RegisterPlotter(cts.Token);

            if (string.IsNullOrEmpty(plotId))
            {
                var running = q.Where(p => p.PlotState == PlotState.RUNNING);
                var count = running.Count();
                ConsoleMessage.MarkupLine($"There {(count == 1 ? "is" : "are")} [wheat1]{count}[/] running plot job{(count == 1 ? "" : "s")}");
                foreach (var plot in running)
                {
                    ConsoleMessage.MarkupLine($"Log for plot [wheat1]{plot.Id}[/]");
                    ConsoleMessage.WriteLine(plot.Log);
                    ConsoleMessage.WriteLine("");
                }
            }
            else
            {
                var plot = q.FirstOrDefault(p => p.Id == plotId);
                if (plot is null)
                {
                    throw new InvalidOperationException($"No plot with an id of {plotId} was found");
                }

                ConsoleMessage.WriteLine(plot.Log);
                ConsoleMessage.WriteLine("");
            }
        }
    }
}
