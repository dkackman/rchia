using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.PlotNft
{
    internal sealed class InspectNftCmmand : WalletCommand
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Wallet);
                var wallet = await LoginToWallet(rpcClient);
                var tasks = new PlotNftTasks(wallet, this);

                await tasks.Inspect(Id);
            });
        }
    }
}
