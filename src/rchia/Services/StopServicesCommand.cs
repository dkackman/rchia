﻿using System;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Services
{
    internal sealed class StopServicesCommand : EndpointOptions
    {
        [Argument(0, Name = "service-group", Description = "[all|node|harvester|farmer|farmer-no-wallet|farmer-only|timelord|\ntimelord-only|timelord-launcher-only|wallet|wallet-only|introducer|simulator]")]
        public string? ServiceGroup { get; init; }

        [Option("d", "daemon", Description = "Stop the daemon service as well\nThe daemon cannot be restarted remotely")]
        public bool Daemon { get; init; }

        [Option("f", "force", Default = false, Description = "If specified in conjunstion with '-d', shut down the daemon without prompting")]
        public bool Force { get; init; }

        [CommandTarget]
        public async Task<int> Run()
        {
            if (ServiceGroup is null || !ServiceGroups.Groups.ContainsKey(ServiceGroup))
            {
                throw new InvalidOperationException($"Unrecognized service group {ServiceGroup}. It must be one of\n  {string.Join("|", ServiceGroups.Groups.Keys)}.");
            }

            if (Daemon)
            {
                if (!Confirm("The daemon cannot be restared remotely. You will need shell access to the node in order to restart it.\nAre you sure you want to stop the daemon?", Force))
                {
                    throw new Exception("No services were stopped.");
                }
            }

            return await DoWorkAsync("Stopping services...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(ctx, this);

                var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);

                foreach (var service in ServiceGroups.Groups[ServiceGroup])
                {
                    using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                    var isRunnnig = await proxy.IsServiceRunning(service, cts.Token);

                    if (isRunnnig)
                    {
                        MarkupLine($"Stopping [wheat1]{service}[/]...");
                        await proxy.StopService(service, cts.Token);
                    }
                    else
                    {
                        MarkupLine($"[wheat1]{service}[/] is not running...");
                    }
                }

                if (Daemon)
                {
                    using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                    await proxy.Exit(cts.Token);
                }
            });
        }
    }
}