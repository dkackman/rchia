using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Wallet
{
    internal sealed class DeleteUnconfirmedTransactionsCommand : WalletCommand
    {
        [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
        public uint Id { get; init; } = 1;

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await DoWorkAsync("Deleting unconfirmed transactions...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Wallet);
                var wallet = new chia.dotnet.Wallet(Id, await Login(rpcClient));

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                await wallet.DeleteUnconfirmedTransactions(cts.Token);

                MarkupLine($"Successfully deleted all unconfirmed transactions for wallet id [wheat1]{Id}[/]");
            });
        }
    }
}
