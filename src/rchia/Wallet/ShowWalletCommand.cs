using System.Threading.Tasks;
using rchia.Commands;

namespace rchia.Wallet
{
    internal sealed class ShowWalletCommand : WalletCommand
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var tasks = new WalletTasks(await Login(), this);
                await DoWork("Retrieving wallet info...", async ctx => { await tasks.Show(); });
            });
        }
    }
}
