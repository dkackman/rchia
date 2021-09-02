using System;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Farm
{
    [Command("farm", Description = "Manage your farm.\nRequires a daemon endpoint.")]
    internal sealed class FarmCommand : SharedOptions
    {
        [Command("challenges", Description = "Show the latest challenges")]
        public CallengesCommand Challenges { get; set; } = new();

        [Option("s", "summary", Description = "Summary of farming information")]
        public bool Summary { get; set; }

        public override async Task<int> Run()
        {
            try
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Daemon);
                var daemon = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new FarmTasks(daemon, this);

                if (Summary)
                {
                    await tasks.Summary();
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
