using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;

namespace rchia.Status
{
    internal sealed class ServicesCommand : EndpointOptions
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var tasks = await CreateTasksWithDaemon<StatusTasks>(ServiceNames.Daemon);

                await DoWork("Retrieving service info...", async ctx => { await tasks.Services(); });
            });
        }
    }
}
