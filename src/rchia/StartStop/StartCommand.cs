using System;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.StartStop
{
    [Command("start", Description = "Start service groups.\nRequires a daemon endpoint.")]
    internal sealed class StartCommand : EndpointOptions
    {
        [Argument(0, Name = "service-group", Description = "[all|node|harvester|farmer|farmer-no-wallet|farmer-only|timelord|\ntimelord-only|timelord-launcher-only|wallet|wallet-only|introducer|simulator]")]
        public string? ServiceGroup { get; init; }

        [Option("r", "restart", Description = "Restart the specified service(s)")]
        public bool Restart { get; init; }

        [CommandTarget]
        public async Task<int> Run()
        {
            if (ServiceGroup is null || !ServiceGroups.Groups.ContainsKey(ServiceGroup))
            {
                throw new InvalidOperationException($"Unrecognized service group {ServiceGroup}. It must be one of\n  {string.Join('|', ServiceGroups.Groups.Keys)}.");
            }

            return await DoWorkAsync("Starting services...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(ctx, this);

                var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);

                foreach (var service in ServiceGroups.Groups[ServiceGroup])
                {
                    using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                    var isRunnnig = await proxy.IsServiceRunning(service, cts.Token);

                    if (isRunnnig && !Restart)
                    {
                        MarkupLine($"[wheat1]{service}[/] is already running. Use -r to restart it...");
                    }
                    else
                    {
                        if (isRunnnig && Restart)
                        {
                            MarkupLine($"Stopping [wheat1]{service}[/]...");
                            using var cts2 = new CancellationTokenSource(TimeoutMilliseconds);
                            await proxy.StopService(service, cts2.Token);
                        }

                        MarkupLine($"Starting [wheat1]{service}[/]...");
                        using var cts3 = new CancellationTokenSource(TimeoutMilliseconds);
                        await proxy.StartService(service, cts3.Token);
                    }
                }
            });
        }
    }
}
