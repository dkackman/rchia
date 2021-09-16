using System;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.PlotNft
{
    internal sealed class CreatePlotNftCommand : WalletCommand
    {
        [Option("u", "pool-url", Description = "HTTPS host:port of the pool to join. Omit for self pooling")]
        public Uri? PoolUrl { get; set; }

        [Option("s", "state", IsRequired = true, Description = "Initial state of Plot NFT")]
        public InitialPoolingState State { get; set; }

        [Option("f", "force", Default = false, Description = "Do not prompt before nft creation")]
        public bool Force { get; set; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Wallet);
                var wallet = await LoginToWallet(rpcClient);
                var tasks = new PlotNftTasks(wallet, this);

                var msg = await tasks.ValidatePoolingOptions(State, PoolUrl);
                if (Confirm(msg, Force))
                {
                    await tasks.Create(State, PoolUrl);
                }
            });
        }
    }
}
