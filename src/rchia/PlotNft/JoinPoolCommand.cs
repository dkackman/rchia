using System;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
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
        public async Task<int> Run()
        {
            return await DoWorkAsync("Joining pool...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Wallet);
                var proxy = await Login(rpcClient, ctx);
                var msg = await proxy.ValidatePoolingOptions(true, PoolUrl, TimeoutMilliseconds);
                if (Confirm(msg, Force))
                {
                    using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                    var wallet = new PoolWallet(Id, proxy);
                    await wallet.Validate(cts.Token);

                    var poolInfo = await PoolUrl.GetPoolInfo(TimeoutMilliseconds);
                    var tx = await wallet.JoinPool(poolInfo.TargetPuzzleHash ?? string.Empty, PoolUrl.ToString(), poolInfo.RelativeLockHeight, cts.Token);

                    PrintTransactionSentTo(tx);
                }
            });
        }
    }
}
