using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.PlotNft
{
    internal sealed class ShowPlotNftCommand : SharedOptions
    {
        [Option("fp", "fingerprint", Description = "Set the fingerprint to specify which wallet to use")]
        public uint? Fingerprint { get; set; }

        [Option("i", "id", Description = "Id of the wallet to use")]
        public uint? Id { get; set; }

        private async Task<uint?> GetWalletId(WalletProxy wallet)
        {
            if (Fingerprint.HasValue)
            {
                return await wallet.GetWalletId(Fingerprint.Value);
            }

            return Id;
        }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Wallet);
                var wallet = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new PlotNftTasks(wallet, this);

                await tasks.Show(await GetWalletId(wallet));
            });
        }
    }
}
