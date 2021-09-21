using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;

namespace rchia.Farm
{
    internal sealed class SummaryCommand : EndpointOptions
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var tasks = await CreateTasksWithDaemon<FarmTasks>(ServiceNames.Farmer);

                await DoWork("Retrieving farm info...", async ctx => { await tasks.Summary(); });
            });
        }
    }
}
