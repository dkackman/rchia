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
                if (Confirm("Deleting all of your keys CANNOT be undone.", Force))
                {
                    using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Wallet);
                    var wallet = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                    var tasks = new KeysTasks(wallet, this);

                    Message("Deleting all keys...");
                    await tasks.DeleteAll();
                    Message("All keys deleted.");
                }
            });
        }
    }
}
