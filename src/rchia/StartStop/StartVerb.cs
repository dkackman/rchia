﻿using System;
using System.Threading.Tasks;

using chia.dotnet;
using chia.dotnet.console;

namespace rchia.StartStop
{
    [Command("start", Description = "Start service groups.\nRequires a daemon endpoint.")]
    internal sealed class StartVerb : SharedOptions
    {
        [Argument(0, Name = "service-group", Description = "[all|node|harvester|farmer|farmer-no-wallet|farmer-only|timelord|\ntimelord-only|timelord-launcher-only|wallet|wallet-only|introducer|simulator]")]
        public string? ServiceGroup { get; set; }

        [Option('r', "restart", Description = "Restart the specified service(s)")]
        public bool Restart { get; set; }

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

                    await commands.Start(ServiceGroup, Restart);
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
