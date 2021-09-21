using System.Threading.Tasks;
using rchia.Commands;

namespace rchia.PlotNft
{
    internal sealed class LeavePoolCommand : WalletCommand
    {
        [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
        public uint Id { get; init; } = 1;

        [Option("f", "force", Default = false, Description = "Do not prompt before nft creation")]
        public bool Force { get; init; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                if (Confirm($"Are you sure you want to start self-farming with Plot NFT on wallet id {Id}?", Force))
                {
                    using var tasks = new PlotNftTasks(await Login(), this);
                    await DoWork("Leaving pool...", async ctx => await tasks.LeavePool(Id));
                }
            });
        }
    }
}
