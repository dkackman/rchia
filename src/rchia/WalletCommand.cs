using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia
{
    internal abstract class WalletCommand : EndpointOptions
    {
        [Option("fp", "fingerprint", Description = "Set the fingerprint to specify which wallet to use - the first fingerprint will be used if not set")]
        public uint? Fingerprint { get; set; }

        protected async Task<WalletProxy> Login()
        {
            var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, ServiceNames.Wallet);
            return await LoginToWallet(rpcClient);
        }

        protected async Task<WalletProxy> LoginToWallet(IRpcClient rpcClient)
        {
            using var cts = new CancellationTokenSource(30000);
            var walletProxy = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
            if (Fingerprint.HasValue)
            {
                _ = await walletProxy.LogIn(Fingerprint.Value, false, cts.Token);
            }
            else
            {
                _ = await walletProxy.LogIn(false, cts.Token);
            }

            return walletProxy;
        }
    }
}
