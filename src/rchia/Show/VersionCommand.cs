using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Show
{
    internal sealed class VersionCommand : EndpointOptions
    {
        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Retrieving chia version...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(ctx, this);
                var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var version = await proxy.GetVersion(cts.Token);

                MarkupLine($"[wheat1]{version}[/]");
            });
        }
    }
}
