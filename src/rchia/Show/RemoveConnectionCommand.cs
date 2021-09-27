using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Show
{
    internal sealed class RemoveConnectionCommand : EndpointOptions
    {
        [Argument(0, Name = "ID", Description = "The full or first 8 characters of NodeID")]
        public string NodeID { get; init; } = string.Empty;

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await DoWorkAsync("Removing connection...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.FullNode);
                var proxy = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                await proxy.CloseConnection(NodeID, cts.Token);
            });
        }
    }
}
