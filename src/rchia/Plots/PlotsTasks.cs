using System;
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
            Console.WriteLine("Add with 'chia plots add [dir]' and remove with 'chia plots remove [dir]' Scan and check plots with 'chia plots check'");
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
