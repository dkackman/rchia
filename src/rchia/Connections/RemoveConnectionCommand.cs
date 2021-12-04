using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Connections;

internal sealed class RemoveConnectionCommand : EndpointOptions
{
    [Argument(0, Name = "ID", Description = "The full or first 8 characters of NodeID")]
    public string NodeID { get; init; } = string.Empty;

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Removing connection...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.FullNode);
            var proxy = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            await proxy.CloseConnection(NodeID, cts.Token);
        });
    }
}
