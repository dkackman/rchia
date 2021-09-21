using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;

namespace rchia.Keys
{
    internal sealed class GenerateKeyCommand : EndpointOptions
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var tasks = await CreateTasks<KeysTasks, WalletProxy>(ServiceNames.Wallet);

                await DoWork("Generating a new key...", async ctx => { await tasks.Generate(); });
            });
        }
    }
}
