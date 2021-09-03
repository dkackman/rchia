using System;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Status
{
    [Command("status", Description = "Shows the status of the node.\nRequires a daemon endpoint.")]
    internal sealed class StatusCommand : SharedOptions
    {
        [Option("s", "services", Description = "Show which services are running on the node")]
        public bool Services { get; set; }

        [CommandTarget]
        public override async Task<int> Run()
        {
            try
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Daemon);
                var daemon = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new StatusTasks(daemon, this);

                if (Services)
                {
                    await tasks.Services();
                }
                else
                {
                    throw new InvalidOperationException("Unrecognized command");
                }

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
