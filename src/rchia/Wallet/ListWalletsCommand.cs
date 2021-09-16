using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Wallet
{
    internal sealed class ListWalletsCommand : EndpointOptions
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, ServiceNames.Wallet);
            var wallet = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
            var tasks = new WalletTasks(wallet, this);

            return await Execute(async () =>
            {
                await DoWork("Retrieving wallet list...", async ctx => { await tasks.List(); });
            });
        }
    }
}
