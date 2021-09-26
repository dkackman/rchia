using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.PlotNft
{
    internal sealed class ClaimNftCommand : WalletCommand
    {
        [Option("f", "force", Default = false, Description = "Do not prompt before claiming rewards")]
        public bool Force { get; init; }

        [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
        public uint Id { get; init; } = 1;

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await DoWork2("Claiming pool rewards...", async ctx =>
            {
                if (Confirm($"Are you sure you want to claim rewards for wallet ID {Id}?", Force))
                {
                    using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Wallet);
                    var wallet = new PoolWallet(Id, await Login(rpcClient));

                    using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                    await wallet.Validate(cts.Token);

                    var (State, tx) = await wallet.AbsorbRewards(0, cts.Token);

                    PrintTransactionSentTo(tx);
                }
            });
        }
    }
}
