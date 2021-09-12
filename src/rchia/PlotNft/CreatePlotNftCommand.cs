using System;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.PlotNft
{
    internal sealed class CreatePlotNftCommand : SharedOptions
    {
        [Option("u", "pool-url", Description = "HTTPS host:port of the pool to join. Omit for self pooling.")]
        public string PoolUrl { get; set; } = string.Empty;

        [Option("s", "state", IsRequired = true, Description = "HTTPS host:port of the pool to join. Omit for self pooling.")]
        public InitialPoolingState State { get; set; }

        [Option("f", "force", Default = false, Description = "Do not prompt before nft creation")]
        public bool Force { get; set; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Wallet);
                var wallet = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new PlotNftTasks(wallet, this);

                var uri = !string.IsNullOrEmpty(PoolUrl) ? new Uri(PoolUrl) : null;
                var msg = await tasks.CheckCreate(State, uri);
                if (Confirm(msg, "Are you sure?", Force))
                {
                    await tasks.Create(State, uri);
                }

            });
        }
    }
}
