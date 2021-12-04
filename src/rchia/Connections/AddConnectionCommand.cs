using System;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Connections;

internal sealed class AddConnectionCommand : EndpointOptions
{
    [Argument(0, Name = "host", Description = "Full Node ip:port")]
    public string HostUri { get; init; } = string.Empty;

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Adding connection...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.FullNode);
            var proxy = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var uri = HostUri.StartsWith("http") ? new Uri(HostUri) : new Uri("https://" + HostUri); // need to add a scheme so uri can be parsed
            await proxy.OpenConnection(uri.Host, uri.Port, cts.Token);
        });
    }
}
