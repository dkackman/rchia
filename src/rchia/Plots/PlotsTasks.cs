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

        public async Task List()
        {
            using var cts = new CancellationTokenSource(10000);

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
