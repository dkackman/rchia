using System;
using System.Collections.Generic;
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

        [Option("f", "force", Description = "If specified in conjunction with '-d', shut down the daemon without prompting")]
        public bool Force { get; init; }

        protected async override Task<bool> Confirm(ICommandOutput output)
        {
            await Task.CompletedTask;

            if (Daemon && !output.Confirm("The daemon cannot be restared remotely. You will need shell access to the node in order to restart it.\nAre you sure you want to stop the daemon?", Force))
            {
                return false;
            }

            return true;
        }

        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Stopping services...", async output =>
            {
                if (ServiceGroup is null || !ServiceGroups.Groups.ContainsKey(ServiceGroup))
                {
                    throw new InvalidOperationException($"Unrecognized service group {ServiceGroup}. It must be one of\n  {string.Join("|", ServiceGroups.Groups.Keys)}.");
                }

                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(output, this);
                var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);

                var result = new List<string>();
                foreach (var service in ServiceGroups.Groups[ServiceGroup])
                {
                    using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                    var isRunnnig = await proxy.IsServiceRunning(service, cts.Token);

                    if (isRunnnig)
                    {
                        output.WriteMarkupLine($"Stopping [wheat1]{service}[/]...");
                        await proxy.StopService(service, cts.Token);
                    }
                    else
                    {
                        output.WriteMarkupLine($"[wheat1]{service}[/] is not running...");
                    }

                    result.Add(service);
                }

                if (Daemon)
                {
                    using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                    await proxy.Exit(cts.Token);
                    result.Add("daemon");
                }

                output.WriteOutput(result);
            });
        }
    }
}
