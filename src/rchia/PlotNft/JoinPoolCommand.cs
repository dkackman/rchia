using System;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.PlotNft
{
    internal sealed class JoinPoolCommand : WalletCommand
    {
        [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
        public uint Id { get; set; } = 1;

        [Option("u", "pool-url", Description = "HTTPS host:port of the pool to join.")]
        public Uri PoolUrl { get; set; } = new Uri("http://localhost");

        [Option("f", "force", Default = false, Description = "Do not prompt before joining")]
        public bool Force { get; set; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Wallet);
                var wallet = await LoginToWallet(rpcClient);
                var tasks = new PlotNftTasks(wallet, this);

                var msg = await tasks.ValidatePoolingOptions(InitialPoolingState.pool, PoolUrl);
                if (Confirm(msg, Force))
                {
                    await tasks.Join(Id, PoolUrl);
                }
            });
        }
    }
}
