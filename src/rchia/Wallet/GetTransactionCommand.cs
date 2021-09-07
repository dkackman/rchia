using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Wallet
{
    internal sealed class GetTransactionCommand : SharedOptions
    {
        [Option("tx", "tx-id", IsRequired = true, Description = "Transaction id to search for")]
        public string TxId { get; set; } = string.Empty;

        [CommandTarget]
        public override async Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, ServiceNames.Wallet);
                var wallet = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new WalletTasks(wallet, this);

                await tasks.GetTransaction(TxId);
            });
        }
    }
}
