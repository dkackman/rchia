using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using chia.dotnet;

namespace rchia.Status
{
    internal class StatusTasks
    {
        public StatusTasks(DaemonProxy daemon, bool verbose)
        {
            Daemon = daemon;
            Verbose = verbose;
        }

        public DaemonProxy Daemon { get; init; }

        public bool Verbose { get; init; }

        public async Task Services()
        {
            // get all the service names from chia.dotnet
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
