using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Keys
{
    internal sealed class GenerateAndPrintKeyCommand : EndpointOptions
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, ServiceNames.Wallet);
                var proxy = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new KeysTasks(proxy, this);

                await DoWork("Generating a new key...", async ctx => { await tasks.GenerateAndPrint(); });
            });
        }
    }
}
