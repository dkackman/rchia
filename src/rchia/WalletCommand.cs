using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Bech32;
using rchia.Commands;
using Spectre.Console;

namespace rchia
{
    internal abstract class WalletCommand : EndpointOptions
    {
        [Option("fp", "fingerprint", Description = "Set the fingerprint to specify which wallet to use - the first fingerprint will be used if not set")]
        public uint? Fingerprint { get; init; }

        protected static Table CreateTransactionTable()
        {
            var table = new Table();
            table.AddColumn("[orange3]Name[/]");
            table.AddColumn("[orange3]Status[/]");
            table.AddColumn("[orange3]Amount[/]");
            table.AddColumn("[orange3]To[/]");
            table.AddColumn("[orange3]At[/]");
            return table;
        }

        protected void PrintTransactionSentTo(TransactionRecord tx)
        {
            NameValue("Transaction", tx.Name);
            foreach (var sentTo in tx.SentTo)
            {
                NameValue($"Sent to", sentTo.Peer);
            }

            Helpful($"Do '[grey]rchia wallet get-transaction -tx {tx.Name}[/]' to get status");
        }

        protected void PrintTransaction(TransactionRecord tx, string prefix, Table table)
        {
            var name = Verbose ? tx.Name : tx.Name.Substring(2, 10) + "...";
            var status = tx.Confirmed ? "Confirmed"
                : tx.IsInMempool ? "In mempool"
                : "Pending";

            var color = tx.Sent > 0 ? "red" : "green";

            var amount = $"[{color}]{tx.Amount.AsChia()} {prefix}[/]";
            var bech32 = new Bech32M(prefix);
            var to = Verbose ? bech32.PuzzleHashToAddress(tx.ToPuzzleHash) : bech32.PuzzleHashToAddress(tx.ToPuzzleHash).Substring(prefix.Length, 10) + "...";
            var at = tx.CreatedAtDateTime.ToLocalTime().ToString();

            table.AddRow(name, status, amount, to, at);
        }

        protected async Task<WalletProxy> Login(IRpcClient rpcClient)
        {
            var walletProxy = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            if (Fingerprint.HasValue)
            {
                _ = await walletProxy.LogIn(Fingerprint.Value, false, cts.Token);
            }
            else
            {
                _ = await walletProxy.LogIn(false, cts.Token);
            }

            return walletProxy;
        }
    }
}
