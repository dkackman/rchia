using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Node;

internal sealed class StopNodeCommand : EndpointOptions
{
    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Shutting down the node...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.FullNode);
            var proxy = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            await proxy.StopNode(cts.Token);

            output.WriteOutput("stopped", rpcClient.Endpoint.Uri, true);
        });
    }
}
