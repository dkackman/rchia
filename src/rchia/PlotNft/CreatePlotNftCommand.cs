using System;
using System.Threading.Tasks;
using rchia.Commands;

namespace rchia.PlotNft
{
    internal sealed class CreatePlotNftCommand : WalletCommand
    {
        [Option("u", "pool-url", Description = "HTTPS host:port of the pool to join. Omit for self pooling")]
        public Uri? PoolUrl { get; init; }

        [Option("s", "state", IsRequired = true, Description = "Initial state of Plot NFT")]
        public InitialPoolingState State { get; init; }

        [Option("f", "force", Default = false, Description = "Do not prompt before nft creation")]
        public bool Force { get; init; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var tasks = new PlotNftTasks(await Login(), this, TimeoutMilliseconds);

                var msg = await tasks.ValidatePoolingOptions(State, PoolUrl);
                if (Confirm(msg, Force))
                {
                    await DoWork("Creating pool NFT and wallet...", async ctx => await tasks.Create(State, PoolUrl));
                }
            });
        }
    }
}
