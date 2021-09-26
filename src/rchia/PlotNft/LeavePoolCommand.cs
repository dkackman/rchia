using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
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
            return await DoWork2("Leaving pool...", async ctx =>
            {
                if (Confirm($"Are you sure you want to start self-farming with Plot NFT on wallet id {Id}?", Force))
                {
                    using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Wallet);

                    using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                    var wallet = new PoolWallet(Id, await Login(rpcClient));
                    await wallet.Validate(cts.Token);

                    var tx = await wallet.SelfPool(cts.Token);

                    PrintTransactionSentTo(tx);
                }
            });
        }
    }
}
