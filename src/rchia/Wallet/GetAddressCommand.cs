using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Wallet
{
    internal sealed class GetAddressCommand : WalletCommand
    {
        [Option("n", "new", Default = false, Description = "Flag indicating whether to create a new address")]
        public bool New { get; set; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, ServiceNames.Wallet);
                var wallet = await LoginToWallet(rpcClient);
                var tasks = new WalletTasks(wallet, this);

                await tasks.GetAddress(Id, New);
            });
        }
    }
}
