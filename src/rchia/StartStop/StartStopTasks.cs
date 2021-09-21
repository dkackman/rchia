using System.Threading;
using System.Threading.Tasks;

using chia.dotnet;
using rchia.Commands;

namespace rchia.StartStop
{
    internal sealed class StartStopTasks : ConsoleTask<DaemonProxy>
    {
        public StartStopTasks(DaemonProxy daemon, IConsoleMessage consoleMessage, int timeoutSeconds)
            : base(daemon, consoleMessage, timeoutSeconds)
        {
        }

        public async Task Start(string groupName, bool restart)
        {
            foreach (var service in ServiceGroups.Groups[groupName])
            {
                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var isRunnnig = await Service.IsServiceRunning(service, cts.Token);

                if (isRunnnig && !restart)
                {
                    ConsoleMessage.MarkupLine($"[wheat1]{service}[/] is already running. Use -r to restart it...");
                }
                else
                {
                    if (isRunnnig && restart)
                    {
                        ConsoleMessage.MarkupLine($"Stopping [wheat1]{service}[/]...");
                        using var cts2 = new CancellationTokenSource(TimeoutMilliseconds);
                        await Service.StopService(service, cts2.Token);
                    }

                    ConsoleMessage.MarkupLine($"Starting [wheat1]{service}[/]...");
                    using var cts3 = new CancellationTokenSource(TimeoutMilliseconds);
                    await Service.StartService(service, cts3.Token);
                }
            }
        }

        public async Task Stop(string groupName)
        {
            foreach (var service in ServiceGroups.Groups[groupName])
            {
                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var isRunnnig = await Service.IsServiceRunning(service, cts.Token);

                if (isRunnnig)
                {
                    ConsoleMessage.MarkupLine($"Stopping [wheat1]{service}[/]...");
                    using var cts3 = new CancellationTokenSource(TimeoutMilliseconds);
                    await Service.StopService(service, cts3.Token);
                }
                else
                {
                    ConsoleMessage.MarkupLine($"[wheat1]{service}[/] is not running...");
                }
            }
        }

        public async Task StopDeamon()
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            await Service.Exit(cts.Token);
        }
    }
}
