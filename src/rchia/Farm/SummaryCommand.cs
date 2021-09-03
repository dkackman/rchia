using System;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Farm
{
    internal sealed class SummaryCommand : SharedOptions
    {
        [CommandTarget]
        public override async Task<int> Run()
        {
            try
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Daemon);
                var daemon = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new FarmTasks(daemon, this);

                await tasks.Summary();

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
