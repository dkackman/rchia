using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots
{
    internal class HarvesterPlotTasks : ConsoleTask<HarvesterProxy>
    {
        public HarvesterPlotTasks(HarvesterProxy proxy, IConsoleMessage consoleMessage)
            : base(proxy, consoleMessage)
        {
        }

        public async Task List()
        {
            using var cts = new CancellationTokenSource(30000);

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
            using var cts = new CancellationTokenSource(30000);
            await Service.RemovePlotDirectory(dirname, cts.Token);
        }

        public async Task Refresh()
        {
            using var cts = new CancellationTokenSource(30000);
            await Service.RefreshPlots(cts.Token);
        }

        public async Task Add(string dirname)
        {
            using var cts = new CancellationTokenSource(30000);
            await Service.AddPlotDirectory(dirname, cts.Token);
        }

        public async Task Show()
        {
            Console.WriteLine("Directories where plots are being searched for:");
            Console.WriteLine("Note that subdirectories must be added manually");
            Console.WriteLine("Add with 'chia plots add [dir]' and remove with 'chia plots remove [dir]'");
            Console.WriteLine("");

            using var cts = new CancellationTokenSource(30000);
            var directories = await Service.GetPlotDirectories(cts.Token);
            foreach (var path in directories)
            {
                Console.WriteLine($"{path}");
            }
        }
    }
}
