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
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, ServiceNames.Harvester);
                var proxy = new HarvesterProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new HarvesterPlotTasks(proxy, this);

                await DoWork("Retrieving plot info...", async ctx => { await tasks.Show(); });
            });
        }
    }
}
