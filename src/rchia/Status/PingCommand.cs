using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;

namespace rchia.Status
{
    internal sealed class PingCommand : EndpointOptions
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var tasks = await CreateTasksWithDaemon<StatusTasks>(ServiceNames.Daemon);

                await DoWork("Pinging the daemon...", async ctx => { await tasks.Ping(); });
            });
        }
    }
}
