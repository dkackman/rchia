using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Services;

internal sealed class ListServicesCommand : EndpointOptions
{
    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving service info...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(output, this);
            var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);

            var fields = typeof(ServiceNames).GetFields(BindingFlags.Public | BindingFlags.Static);
            var serviceNames = new ServiceNames();

            var result = new Dictionary<string, object?>();
            foreach (var name in fields.Where(f => f.Name != "Daemon"))
            {
                var service = name.GetValue(serviceNames)?.ToString() ?? string.Empty;
                using var cts = new CancellationTokenSource(TimeoutMilliseconds);

                var isRunning = await proxy.IsServiceRunning(service, cts.Token);
                result.Add(service, isRunning ? new Formattable<string>("running", "green") : new Formattable<string>("not running", "grey"));
            }

            output.WriteOutput(result);
        });
    }
}
