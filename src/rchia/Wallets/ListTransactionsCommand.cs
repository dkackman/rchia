using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Wallet;

internal sealed class ListTransactionsCommand : WalletCommand
{
    [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
    public int Id { get; init; } = 1;

    [Option("s", "start", Default = 0, Description = "The start index of transactions to show")]
    public int Start { get; init; }

    [Option("c", "count", Description = "The max number of transactions to show. If not specified, all transactions will be shown")]
    public int? Count { get; init; }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving transactions...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var wallet = new chia.dotnet.Wallet((uint)Id, await Login(rpcClient, output));

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var (NetworkName, NetworkPrefix) = await wallet.WalletProxy.GetNetworkInfo(cts.Token);

            var count = Count is null ? await wallet.GetTransactionCount(cts.Token) : (uint)Count.Value;
            var transactions = await wallet.GetTransactions((uint)Start, count - (uint)Start, cancellationToken: cts.Token);

            if (transactions.Any())
            {
                var result = new List<IDictionary<string, object?>>();

                foreach (var tx in transactions)
                {
                    var dict = new Dictionary<string, object?>();
                    FormatTransaction(tx, NetworkPrefix, dict);
                    result.Add(dict);
                }

                output.WriteOutput(result);
                var c = transactions.Count();
                output.WriteMarkupLine($"Showing [wheat1]{c}[/] transaction{(c == 1 ? string.Empty : "s")}");
            }
            else
            {
                output.WriteOutput("warning", "There are no transactions to this address", Verbose);
            }
        });
    }
}
