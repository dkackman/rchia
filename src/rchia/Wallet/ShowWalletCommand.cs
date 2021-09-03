using System;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Wallet
{
    internal sealed class ShowWalletCommand : SharedOptions
    {
        [Argument(0, Name = "fingerprint", Description = "Enter the fingerprint of the key you want to delete")]
        public uint Fingerprint { get; set; }

        [CommandTarget]
        public override async Task<int> Run()
        {
            try
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, ServiceNames.Wallet);
                var wallet = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new WalletTasks(wallet, this);

                await tasks.Show(Fingerprint);

                return 0;
            }
            catch (Exception e)
            {
                Message(e);

                return -1;
            }
        }
    }
}
