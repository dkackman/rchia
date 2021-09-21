using System.Threading.Tasks;
using rchia.Commands;

namespace rchia.Wallet
{
    internal sealed class GetTransactionCommand : WalletCommand
    {
        [Option("tx", "tx-id", IsRequired = true, Description = "Transaction id to search for")]
        public string TxId { get; init; } = string.Empty;

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var tasks = new WalletTasks(await Login(), this);
                await DoWork("Retrieving transaction...", async ctx => { await tasks.GetTransaction(TxId); });
            });
        }
    }
}
