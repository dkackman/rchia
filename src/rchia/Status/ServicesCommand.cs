using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Status
{
    internal sealed class ServicesCommand : EndpointOptions
    {
        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Retrieving service info...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(ctx, this);

                var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);

                var fields = typeof(ServiceNames).GetFields(BindingFlags.Public | BindingFlags.Static);
                var serviceNames = new ServiceNames();

                foreach (var name in fields.Where(f => f.Name != "Daemon"))
                {
                    var service = name.GetValue(serviceNames)?.ToString() ?? string.Empty;
                    using var cts = new CancellationTokenSource(TimeoutMilliseconds);

                    var isRunning = await proxy.IsServiceRunning(service, cts.Token);
                    var status = isRunning ? "[green]running[/]" : "[grey]not running[/]";
                    NameValue(service, status);
                }
            });
        }
    }
}
