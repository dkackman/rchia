using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Wallet
{
    internal sealed class ListWalletsCommand : EndpointOptions
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var tasks = await CreateTasks<WalletTasks, WalletProxy>(ServiceNames.Wallet);

                await DoWork("Retrieving wallet list...", async ctx => { await tasks.List(); });
            });
        }
    }
}
