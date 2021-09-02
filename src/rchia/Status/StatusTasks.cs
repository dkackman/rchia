using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

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

        public async Task Services()
        {
            // get all the service names from chia.dotnet.ServiceNames
            var fields = typeof(ServiceNames).GetFields(BindingFlags.Public | BindingFlags.Static);
            var serviceNames = new ServiceNames();

            foreach (var name in fields.Where(f => f.Name != "Daemon"))
            {
                var service = name.GetValue(serviceNames)?.ToString() ?? string.Empty;
                using var cts = new CancellationTokenSource(500);

                var isRunning = await Service.IsServiceRunning(service, cts.Token);
                var status = isRunning ? "running" : "not running";
                Console.WriteLine($"{service,-25}: {status}");
            }
        }
    }
}
