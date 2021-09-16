using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.PlotNft
{
    internal sealed class ClaimNftCommand : WalletCommand
    {
        [Option("f", "force", Default = false, Description = "Do not prompt before claiming rewards")]
        public bool Force { get; set; }

        [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
        public uint Id { get; set; } = 1;

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                if (Confirm($"Are you sure you want to claim rewards for wallet ID {Id}?", Force))
                {
                    using var tasks = new PlotNftTasks(await Login(), this);
                    await DoWork("Claiming pool rewards...", async ctx => await tasks.Claim(Id));
                }
            });
        }
    }
}
