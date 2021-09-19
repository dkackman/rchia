using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;

namespace rchia.Keys
{
    internal sealed class GenerateKeyCommand : EndpointOptions
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, ServiceNames.Wallet);
                var proxy = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new KeysTasks(proxy, this);

                await DoWork("Generating a new key...", async ctx => { await tasks.Generate(); });
            });
        }
    }
}
