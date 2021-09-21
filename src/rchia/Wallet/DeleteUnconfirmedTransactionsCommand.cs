using System.Threading.Tasks;
using rchia.Commands;

namespace rchia.Wallet
{
    internal sealed class DeleteUnconfirmedTransactionsCommand : WalletCommand
    {
        [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
        public uint Id { get; init; } = 1;

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var tasks = new WalletTasks(await Login(), this);
                await DoWork("Deleting unconfirmed transactions...", async ctx => { await tasks.DeleteUnconfirmedTransactions(Id); });
            });
        }
    }
}
