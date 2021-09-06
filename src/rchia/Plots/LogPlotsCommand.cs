using System;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Plots
{
    internal sealed class LogPlotsCommand : SharedOptions
    {
        [CommandTarget]
        public override async Task<int> Run()
        {
            try
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Farmer);
                var harvester = new HarvesterProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new PlotsTasks(harvester, this);

                await tasks.Log();

                return 0;
            }
            catch (Exception e)
            {
                Message(e);

                return -1;
            }
        }
    }
}
