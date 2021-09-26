using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.PlotNft
{
    internal sealed class InspectNftCommand : WalletCommand
    {
        [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
        public uint Id { get; init; } = 1;

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await DoWork2("Retrieving nft plot info...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Wallet);
                var wallet = new PoolWallet(Id, await Login(rpcClient));

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                await wallet.Validate(cts.Token);

                var (State, UnconfirmedTransactions) = await wallet.Status(cts.Token);

                WriteLine(State.ToJson());

                foreach (var tx in UnconfirmedTransactions)
                {
                    PrintTransactionSentTo(tx);
                }
            });
        }
    }
}
