using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;

namespace rchia.Keys
{
    internal sealed class ShowKeysCommand : EndpointOptions
    {
        [Option("m", "show-mnemonic-seed", Default = false, Description = "Show the mnemonic seed of the keys")]
        public bool ShowMnemonicSeed { get; init; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var tasks = await CreateTasks<KeysTasks, WalletProxy>(ServiceNames.Wallet);

                await DoWork("Retrieving keys...", async ctx => { await tasks.Show(ShowMnemonicSeed); });
            });
        }
    }
}
