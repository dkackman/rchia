using System;
using System.Threading.Tasks;

using chia.dotnet;
using chia.dotnet.console;

using CommandLine;

namespace rchia.Status
{
    [Verb("status", HelpText = "Shows the status of the node.\nRequires a daemon endpoint.")]
    internal sealed class StatusVerb : SharedOptions
    {
        [Option('s', "services", HelpText = "Show which services are running on the node")]
        public bool Services { get; set; }

        public override async Task<int> Run()
        {
            try
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Daemon);
                var daemon = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new StatusTasks(daemon, Verbose);

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
