using System.Threading.Tasks;
using rchia.Commands;

namespace rchia.Wallet
{
    internal sealed class GetAddressCommand : WalletCommand
    {
        [Option("n", "new", Default = false, Description = "Flag indicating whether to create a new address")]
        public bool New { get; set; }

        [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
        public uint Id { get; set; } = 1;

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var tasks = new WalletTasks(await Login(), this);
                await DoWork("Retrieving wallet address...", async ctx => { await tasks.GetAddress(Id, New); });
            });
        }
    }
}
