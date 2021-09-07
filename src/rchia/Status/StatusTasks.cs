using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Status
{
    internal class StatusTasks : ConsoleTask<DaemonProxy>
    {
        public StatusTasks(DaemonProxy daemon, IConsoleMessage consoleMessage)
            : base(daemon, consoleMessage)
        {
        }
        public async Task Ping()
        {
            using var cts = new CancellationTokenSource(20000);

            Console.WriteLine("Pinging the daemon...");

            var stopWatch = Stopwatch.StartNew();
            await Service.Ping();
            stopWatch.Stop();

            Console.WriteLine($"Ping response received after {stopWatch.ElapsedMilliseconds / 1000.0:N2} seconds");
        }

        public async Task Services()
        {
            // get all the service names from chia.dotnet.ServiceNames
            var fields = typeof(ServiceNames).GetFields(BindingFlags.Public | BindingFlags.Static);
            var serviceNames = new ServiceNames();

            foreach (var name in fields.Where(f => f.Name != "Daemon"))
            {
                var service = name.GetValue(serviceNames)?.ToString() ?? string.Empty;
                using var cts = new CancellationTokenSource(5000);

                var isRunning = await Service.IsServiceRunning(service, cts.Token);
                var status = isRunning ? "running" : "not running";
                Console.WriteLine($"{service,-25}: {status}");
            }
        }
    }
}
