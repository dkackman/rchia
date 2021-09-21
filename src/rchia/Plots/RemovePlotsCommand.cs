using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots
{
    internal sealed class RemovePlotsCommand : EndpointOptions
    {
        [Option("d", "final-dir", Default = ".", Description = "Final directory for plots (relative or absolute)")]
        public string FinalDir { get; init; } = ".";

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var tasks = await CreateTasks<HarvesterPlotTasks, HarvesterProxy>(ServiceNames.Harvester);

                await DoWork("Removing plot directory...", async ctx => { await tasks.Remove(FinalDir); });
            });
        }
    }
}
