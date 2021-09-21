using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots
{
    internal sealed class PlotQueueCommand : EndpointOptions
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Plotter, TimeoutMilliseconds);
                var proxy = new PlotterProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new PlotterTasks(proxy, this, TimeoutMilliseconds);

                await DoWork("Retrieivng plot queue...", async ctx => { await tasks.Queue(); });
            });
        }
    }
}
