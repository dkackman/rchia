using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Wallet;

internal sealed class GetTransactionCommand : WalletCommand
{
    [Option("tx", "txid", IsRequired = true, Description = "Transaction id to search for")]
    public string TxId { get; init; } = string.Empty;

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving transaction...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var proxy = await Login(rpcClient, output);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var tx = await proxy.GetTransaction(TxId, cts.Token);
            var (NetworkName, NetworkPrefix) = await proxy.GetNetworkInfo(cts.Token);

            var result = new Dictionary<string, object?>();
            FormatTransaction(tx, NetworkPrefix, result);
            output.WriteOutput(result);
        });
    }
}
