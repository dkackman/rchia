using System;
using System.Threading;
using System.Threading.Tasks;

using chia.dotnet;
using rchia.Commands;

namespace rchia.StartStop
{
    internal sealed class StartStopTasks : ConsoleTask<DaemonProxy>
    {
        public StartStopTasks(DaemonProxy daemon, IConsoleMessage consoleMessage)
            : base(daemon, consoleMessage)
        {
        }

        public async Task Start(string groupName, bool restart)
        {
            foreach (var service in ServiceGroups.Groups[groupName])
            {
                using var cts = new CancellationTokenSource(20000);
                var isRunnnig = await Service.IsServiceRunning(service, cts.Token);

                if (isRunnnig && !restart)
                {
                    Console.WriteLine($"{service} is already running. Use -r to restart it...");
                }
                else
                {
                    if (isRunnnig && restart)
                    {
                        Console.WriteLine($"Stopping {service}...");
                        using var cts2 = new CancellationTokenSource(30000);
                        await Service.StopService(service, cts2.Token);
                    }
                    Console.WriteLine($"Starting {service}...");
                    using var cts3 = new CancellationTokenSource(30000);
                    await Service.StartService(service, cts3.Token);
                }
            }
        }

        public async Task Stop(string groupName)
        {
            foreach (var service in ServiceGroups.Groups[groupName])
            {
                using var cts = new CancellationTokenSource(20000);
                var isRunnnig = await Service.IsServiceRunning(service, cts.Token);

                if (isRunnnig)
                {
                    Console.WriteLine($"Stopping {service}...");
                    using var cts3 = new CancellationTokenSource(30000);
                    await Service.StopService(service, cts3.Token);
                }
                else
                {
                    Console.WriteLine($"{service} is not running...");
                }
            }
        }

        public async Task StopDeamon()
        {
            using var cts = new CancellationTokenSource(20000);
            await Service.Exit(cts.Token);
        }
    }
}
