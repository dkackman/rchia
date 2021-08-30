using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using chia.dotnet;
using chia.dotnet.console;

namespace rchia.Status
{
    internal class StatusTasks : ConsoleTask
    {
        public StatusTasks(DaemonProxy daemon, IConsoleMessage consoleMessage)
            : base(consoleMessage)
        {
            Daemon = daemon;
        }

        public DaemonProxy Daemon { get; init; }

        public async Task Services()
        {
            // get all the service names from chia.dotnet.ServiceNames
            var fields = typeof(ServiceNames).GetFields(BindingFlags.Public | BindingFlags.Static);
            var serviceNames = new ServiceNames();

            foreach (var name in fields.Where(f => f.Name != "Daemon"))
            {
                var service = name.GetValue(serviceNames)?.ToString() ?? string.Empty;
                using var cts = new CancellationTokenSource(500);

                var isRunning = await Daemon.IsServiceRunning(service, cts.Token);
                var status = isRunning ? "running" : "not running";
                Console.WriteLine($"{service,-25}: {status}");
            }
        }
    }
}
