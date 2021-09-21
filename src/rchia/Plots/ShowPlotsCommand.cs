using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots
{
    internal sealed class ShowPlotsCommand : EndpointOptions
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var tasks = await CreateTasks<HarvesterPlotTasks, HarvesterProxy>(ServiceNames.Harvester);

                await DoWork("Retrieving plot info...", async ctx => { await tasks.Show(); });
            });
        }
    }
}
