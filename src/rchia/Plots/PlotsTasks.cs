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


        public async Task Log()
        {
            using var cts = new CancellationTokenSource(20000);

            var plotter = new PlotterProxy((WebSocketRpcClient)Service.RpcClient, Service.OriginService);
            var q = await plotter.RegisterPlotter(cts.Token);
            var last = q.LastOrDefault(p => p.PlotState == PlotState.RUNNING);
            if (last is null)
            {
                Console.WriteLine("There are no running plots");
            }
            else
            {
                Console.WriteLine(last.Log);
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
