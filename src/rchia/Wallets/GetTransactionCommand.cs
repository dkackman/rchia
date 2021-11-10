using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Wallet
{
    internal sealed class GetTransactionCommand : WalletCommand
    {
        [Option("tx", "tx-id", IsRequired = true, Description = "Transaction id to search for")]
        public string TxId { get; init; } = string.Empty;

        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Retrieving transaction...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Wallet);
                var proxy = await Login(rpcClient, ctx);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var tx = await proxy.GetTransaction(TxId, cts.Token);
                var (NetworkName, NetworkPrefix) = await proxy.GetNetworkInfo(cts.Token);

                PrintTransaction(tx, NetworkPrefix, CreateTransactionTable());
            });
        }
    }
}
