using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Wallet
{
    internal sealed class ShowWalletCommand : SharedOptions
    {
        [Option("fp", "fingerprint", Description = "Set the fingerprint to specify which wallet to use")]
        public uint Fingerprint { get; set; }

        [Option("i", "id", Default = 1, Description = "Id of the wallet to use")]
        public uint Id { get; set; } = 1;

        [CommandTarget]
        public override async Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, ServiceNames.Wallet);
                var wallet = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new WalletTasks(wallet, this);

                if (Fingerprint > 0)
                {
                    var idForFingerprint = await wallet.GetWalletId(Fingerprint);
                    await tasks.Show(idForFingerprint);
                }
                else
                {
                    await tasks.Show(Id);
                }
            });
        }
    }
}
