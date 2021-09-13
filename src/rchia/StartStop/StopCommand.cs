using System;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;
using rchia.Endpoints;

namespace rchia.StartStop
{
    [Command("stop", Description = "Stop service groups.\nRequires a daemon endpoint.")]
    internal sealed class StopCommand : EndpointOptions
    {
        [Argument(0, Name = "service-group", Description = "[all|node|harvester|farmer|farmer-no-wallet|farmer-only|timelord|\ntimelord-only|timelord-launcher-only|wallet|wallet-only|introducer|simulator]")]
        public string? ServiceGroup { get; set; }

        [Option("d", "daemon", Description = "Stop the daemon service as well\nThe daemon cannot be restarted remotely")]
        public bool Daemon { get; set; }

        [Option("f", "force", Default = false, Description = "If -d is specified, shut down the daemon without prompting")]
        public bool Force { get; set; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Daemon);
                var daemon = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new StartStopTasks(daemon, this);

                if (ServiceGroup is not null)
                {
                    if (!ServiceGroups.Groups.ContainsKey(ServiceGroup))
                    {
                        throw new InvalidOperationException($"Unrecognized service group {ServiceGroup}. It must be one of\n  {string.Join("|", ServiceGroups.Groups.Keys)}.");
                    }

                    await tasks.Stop(ServiceGroup);
                    if (Daemon)
                    {
                        if (Confirm("The daemon cannot be restared remotely. You will need shell access to the node in order to restart it.", "Are you sure you want to stop the daemon?", Force))
                        {
                            await tasks.StopDeamon();
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("Unrecognized command");
                }
            });
        }
    }
}
