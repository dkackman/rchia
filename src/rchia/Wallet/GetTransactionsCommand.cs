using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Wallet
{
    internal sealed class GetTransactionsCommand : WalletCommand
    {
        [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
        public uint Id { get; set; } = 1;

        [Option("s", "start", Default = 0, Description = "The start index of transactions to show")]
        public uint Start { get; set; }

        [Option("c", "count", Description = "The max number of trasnactions to show. If not specified, all transactions will be shown")]
        public uint? Count { get; set; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, ServiceNames.Wallet);
                var wallet = await LoginToWallet(rpcClient);
                var tasks = new WalletTasks(wallet, this);

                await tasks.GetTransactions(Id, Start, Count);
            });
        }
    }
}
