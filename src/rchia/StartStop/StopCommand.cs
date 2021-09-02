using System;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;
using rchia.Endpoints;

namespace rchia.StartStop
{
    [Command("stop", Description = "Stop service groups.\nRequires a daemon endpoint.")]
    internal sealed class StopCommand : SharedOptions
    {
        [Argument(0, Name = "service-group", Description = "[all|node|harvester|farmer|farmer-no-wallet|farmer-only|timelord|\ntimelord-only|timelord-launcher-only|wallet|wallet-only|introducer|simulator]")]
        public string? ServiceGroup { get; set; }

        [Option("d", "daemon", Description = "Stop the daemon service as well\nThe daemon cannot be restarted remotely")]
        public bool Daemon { get; set; }

        [Option("f", "force", Description = "Do not prompt before stopping the daemon")]
        public bool Force { get; set; }

        public override async Task<int> Run()
        {
            try
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Daemon);
                var daemon = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
                var commands = new StartStopTasks(daemon, this);

                if (ServiceGroup is not null)
                {
                    if (!ServiceGroups.Groups.ContainsKey(ServiceGroup))
                    {
                        throw new InvalidOperationException($"Unrecognized service group {ServiceGroup}. It must be one of\n  {string.Join("|", ServiceGroups.Groups.Keys)}.");
                    }

                    await commands.Stop(ServiceGroup);
                    if (Daemon)
                    {
                        if (Confirm("The daemon cannot be restared remotely. You will need shell access to the node in orer to restart it.", "Are you sure you want to stop the daemon?", Force))
                        {
                            await commands.StopDeamon();
                        }
                    }
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
