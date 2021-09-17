using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Keys
{
    internal sealed class DeleteAllKeys : EndpointOptions
    {
        [Option("f", "force", Default = false, Description = "Delete all keys without prompting for confirmation")]
        public bool Force { get; set; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, ServiceNames.Wallet);
                var proxy = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new KeysTasks(proxy, this);

                if (Confirm($"Deleting all of your keys [bold]CANNOT[/] be undone.\nAre you sure you want to delete all keys from [red]{rpcClient.Endpoint.Uri}[/]?", Force))
                {
                    await DoWork("Deleting all keys...", async ctx => { await tasks.DeleteAll(); });
                }
            });
        }
    }
}
