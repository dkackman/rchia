using System;
using System.Threading.Tasks;
using rchia.Commands;

namespace rchia.PlotNft
{
    internal sealed class JoinPoolCommand : WalletCommand
    {
        [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
        public uint Id { get; init; } = 1;

        [Option("u", "pool-url", Description = "HTTPS host:port of the pool to join.")]
        public Uri PoolUrl { get; init; } = new Uri("http://localhost");

        [Option("f", "force", Default = false, Description = "Do not prompt before joining")]
        public bool Force { get; init; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var tasks = new PlotNftTasks(await Login(), this, TimeoutMilliseconds);

                var msg = await tasks.ValidatePoolingOptions(InitialPoolingState.pool, PoolUrl);
                if (Confirm(msg, Force))
                {
                    await DoWork("Joining pool...", async ctx => await tasks.Join(Id, PoolUrl));
                }
            });
        }
    }
}
