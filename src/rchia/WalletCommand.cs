using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using chia.dotnet;
using rchia.Bech32;
using rchia.Commands;

namespace rchia;

internal abstract class WalletCommand : EndpointOptions
{
    [Option("fp", "fingerprint", Description = "Set the fingerprint to specify which wallet to use - the first fingerprint will be used if not set")]
    public uint? Fingerprint { get; init; }

    protected void PrintTransactionSentTo(ICommandOutput output, TransactionRecord tx)
    {
        var result = new Dictionary<string, object>()
        {
            { "transaction", tx.Name },
            { "sent_t", tx.SentTo.Select(peer => peer.Peer) }
        };
        output.WriteOutput(result);
        output.Helpful($"Do '[grey]rchia wallet get-transaction -tx {tx.Name}' to get status");
    }

    protected void PrintTransaction(TransactionRecord tx, string prefix, IDictionary<string, object> row)
    {
        var name = Verbose ? tx.Name : tx.Name.Substring(2, 10) + "...";
        var status = tx.Confirmed ? "Confirmed"
            : tx.IsInMempool ? "In mempool"
            : "Pending";

        var color = tx.Sent > 0 ? "red" : "green";
        var amount = $"[{color}]{tx.Amount.AsChia()} {prefix}";
        var bech32 = new Bech32M(prefix);
        var to = Verbose ? bech32.PuzzleHashToAddress(tx.ToPuzzleHash) : bech32.PuzzleHashToAddress(tx.ToPuzzleHash).Substring(prefix.Length, 10) + "...";
        var at = tx.CreatedAtDateTime.ToLocalTime().ToString();

        row.Add("name", name);
        row.Add("status", status);
        row.Add("amount", amount);
        row.Add("to", to);
        row.Add("at", at);
    }

    protected async Task<WalletProxy> Login(IRpcClient rpcClient, ICommandOutput output)
    {
        using var message = new StatusMessage(output.Status, "Logging into wallet...");
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
