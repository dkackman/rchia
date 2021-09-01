using System;
using System.Threading.Tasks;

using chia.dotnet;
using chia.dotnet.console;

using CommandLine;

namespace rchia.StartStop
{
    [Verb("stop", HelpText = "Stop service groups.\nRequires a daemon endpoint.")]
    internal sealed class StopVerb : SharedOptions
    {
        [Value(0, MetaName = "service-group", HelpText = "[all|node|harvester|farmer|farmer-no-wallet|farmer-only|timelord|\ntimelord-only|timelord-launcher-only|wallet|wallet-only|introducer|simulator]")]
        public string? ServiceGroup { get; set; }


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
                        throw new InvalidOperationException($"Unrecognized service group {ServiceGroup}. It must be one of\n  {string.Join('|', ServiceGroups.Groups.Keys)}.");
                    }

                    await commands.Stop(ServiceGroup);
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
