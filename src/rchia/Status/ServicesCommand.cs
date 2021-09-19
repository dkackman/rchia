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
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Daemon);
                var daemon = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new StatusTasks(daemon, this);

                await DoWork("Retrieving service info...", async ctx => { await tasks.Services(); });
            });
        }
    }
}
