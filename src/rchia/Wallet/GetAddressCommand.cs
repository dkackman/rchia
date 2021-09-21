using System.Threading.Tasks;
using rchia.Commands;

namespace rchia.Wallet
{
    internal sealed class GetAddressCommand : WalletCommand
    {
        [Option("n", "new", Default = false, Description = "Flag indicating whether to create a new address")]
        public bool New { get; init; }

        [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
        public uint Id { get; init; } = 1;

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var tasks = new WalletTasks(await Login(), this, TimeoutMilliseconds);
                await DoWork("Retrieving wallet address...", async ctx => { await tasks.GetAddress(Id, New); });
            });
        }
    }
}
