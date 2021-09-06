using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;

namespace rchia.Plots
{
    internal class PlotsTasks : ConsoleTask<HarvesterProxy>
    {
        public PlotsTasks(HarvesterProxy harvester, IConsoleMessage consoleMessage)
            : base(harvester, consoleMessage)
        {
        }

        public async Task CreatePlots(PlotterConfig config)
        {
            using var cts = new CancellationTokenSource(20000);

            Console.WriteLine("Queueing plot...");

            var webSocket = (WebSocketRpcClient)Service.RpcClient;
            var plotter = new PlotterProxy(webSocket, Service.OriginService);
            var q = await plotter.RegisterPlotter(cts.Token);
            await plotter.StartPlotting(config, cts.Token);

            Console.WriteLine("Plot queued. Run 'rchia plots queue -v' or 'rchia plots log' to check status");
        }

        public async Task Queue()
        {
            using var cts = new CancellationTokenSource(20000);

            var plotter = new PlotterProxy((WebSocketRpcClient)Service.RpcClient, Service.OriginService);
            var q = await plotter.RegisterPlotter(cts.Token);
            var plots = from p in q
                        group p by p.PlotState into g
                        select g;

            foreach (var group in plots)
            {
                Console.WriteLine($"{group.Count()} {group.Key}");
                if (ConsoleMessage.Verbose)
                {
                    foreach (var item in group)
                    {
                        Console.WriteLine($"  {item.Id}");
                    }
                }
            }
        }

        public async Task Log(string? plotId)
        {
            using var cts = new CancellationTokenSource(20000);

            var plotter = new PlotterProxy((WebSocketRpcClient)Service.RpcClient, Service.OriginService);
            var q = await plotter.RegisterPlotter(cts.Token);

            if (string.IsNullOrEmpty(plotId))
            {
                var running = q.Where(p => p.PlotState == PlotState.RUNNING);
                var count = running.Count();
                Console.Write($"There {(count == 1 ? "is" : "are")} {count} running plot job{(count == 1 ? "" : "s")}");
                foreach (var plot in running)
                {
                    Console.WriteLine($"Log for plot {plot.Id}:");
                    Console.WriteLine(plot.Log);
                    Console.WriteLine("");
                }
            }
            else
            {
                var plot = q.FirstOrDefault(p => p.Id == plotId);
                if (plot is null)
                {
                    throw new InvalidOperationException($"No plot with an id of {plotId} was found");
                }

                Console.WriteLine(plot.Log);
                Console.WriteLine("");
            }
        }

        public async Task List()
        {
            using var cts = new CancellationTokenSource(20000);

            var plots = await Service.GetPlots(cts.Token);

            ListPlots(plots.FailedToOpenFilenames, "failed to open");
            ListPlots(plots.NotFoundFileNames, "not found");
            ListPlots(plots.Plots.Select(p => p.Filename), "plots");

            if (!ConsoleMessage.Verbose)
            {
                ConsoleMessage.Message("(use '-v/--verbose' to see file names)", true);
            }
        }

        private void ListPlots(IEnumerable<string> plots, string msg)
        {
            Console.WriteLine($"{plots.Count()} {msg}.");
            if (plots.Any() && ConsoleMessage.Verbose)
            {
                foreach (var plot in plots)
                {
                    Console.WriteLine(plot);
                }
            }
        }

        public async Task Remove(string dirname)
        {
            using var cts = new CancellationTokenSource(10000);
            await Service.RemovePlotDirectory(dirname, cts.Token);
        }

        public async Task Add(string dirname)
        {
            using var cts = new CancellationTokenSource(10000);
            await Service.AddPlotDirectory(dirname, cts.Token);
        }

        public async Task Show()
        {
            Console.WriteLine("Directories where plots are being searched for:");
            Console.WriteLine("Note that subdirectories must be added manually");
            Console.WriteLine("Add with 'chia plots add [dir]' and remove with 'chia plots remove [dir]'");
            Console.WriteLine("");

            using var cts = new CancellationTokenSource(10000);
            var directories = await Service.GetPlotDirectories(cts.Token);
            foreach (var path in directories)
            {
                Console.WriteLine($"{path}");
            }
        }
    }
}
