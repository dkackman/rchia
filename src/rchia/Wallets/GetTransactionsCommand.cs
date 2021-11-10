using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using Spectre.Console;

namespace rchia.Wallet
{
    internal sealed class GetTransactionsCommand : WalletCommand
    {
        [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
        public uint Id { get; init; } = 1;

        [Option("s", "start", Default = 0, Description = "The start index of transactions to show")]
        public uint Start { get; init; }

        [Option("c", "count", Description = "The max number of trasnactions to show. If not specified, all transactions will be shown")]
        public uint? Count { get; init; }

        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Retrieving transactions...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Wallet);
                var wallet = new chia.dotnet.Wallet(Id, await Login(rpcClient, ctx));

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var (NetworkName, NetworkPrefix) = await wallet.WalletProxy.GetNetworkInfo(cts.Token);

                var count = Count is null ? await wallet.GetTransactionCount(cts.Token) : Count.Value;
                var transactions = await wallet.GetTransactions(Start, count - Start, cts.Token);

                if (transactions.Any())
                {
                    var table = CreateTransactionTable();
                    foreach (var tx in transactions)
                    {
                        PrintTransaction(tx, NetworkPrefix, table);
                    }
                    var c = transactions.Count();
                    AnsiConsole.Write(table);
                    MarkupLine($"Showing [wheat1]{c}[/] transaction{(c == 1 ? string.Empty : "s")}");
                }
                else
                {
                    Warning("There are no transactions to this address");
                }
            });
        }
    }
}
