using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Keys
{
    internal sealed class DeleteAllKeys : EndpointOptions
    {
        [Option("f", "force", Default = false, Description = "Delete all keys without prompting for confirmation")]
        public bool Force { get; init; }

        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Deleting all keys....", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Wallet);

                if (Confirm($"Deleting all of your keys [wheat1]CANNOT[/] be undone.\nAre you sure you want to delete all keys from [red]{rpcClient.Endpoint.Uri}[/]?", Force))
                {
                    var proxy = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                    using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                    await proxy.DeleteAllKeys(cts.Token);

                    MarkupLine("Deleted all keys");
                }
            });
        }
    }
}
