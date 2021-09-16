using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Plots
{
    internal sealed class PlotQueueCommand : EndpointOptions
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Plotter);
                var proxy = new PlotterProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new PlotterTasks(proxy, this);

                await tasks.Queue();
            });
        }
    }
}
