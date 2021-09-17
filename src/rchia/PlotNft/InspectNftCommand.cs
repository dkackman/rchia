using System.Threading.Tasks;
using rchia.Commands;

namespace rchia.PlotNft
{
    internal sealed class InspectNftCommand : WalletCommand
    {
        [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
        public uint Id { get; set; } = 1;

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var tasks = new PlotNftTasks(await Login(), this);
                await DoWork("Retrieving nft plot info...", async ctx => await tasks.Inspect(Id));
            });
        }
    }
}
