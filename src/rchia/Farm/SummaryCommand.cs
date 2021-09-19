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
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Daemon);
                var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new FarmTasks(proxy, this);

                await DoWork("Retrieving farm info...", async ctx => { await tasks.Summary(); });
            });
        }
    }
}
