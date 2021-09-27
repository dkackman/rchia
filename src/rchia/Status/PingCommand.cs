using System.Diagnostics;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Status
{
    internal sealed class PingCommand : EndpointOptions
    {
        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Pinging the daemon...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(ctx, this, ServiceNames.Daemon);

                var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
                var stopWatch = Stopwatch.StartNew();
                await proxy.Ping();
                stopWatch.Stop();

                MarkupLine($"Ping response received after [wheat1]{stopWatch.ElapsedMilliseconds / 1000.0:N2}[/] seconds");
            });
        }
    }
}
