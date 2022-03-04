using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using chia.dotnet.bech32;
using rchia.Commands;

namespace rchia;

internal abstract class WalletCommand : EndpointOptions
{
    [Option("fp", "fingerprint", Description = "Set the fingerprint to specify which wallet to use - the first fingerprint will be used if not set")]
    public uint? Fingerprint { get; init; }

    protected void PrintTransactionSentTo(ICommandOutput output, TransactionRecord tx)
    {
        var result = new Dictionary<string, object?>()
        {
            { "transaction", tx.Name },
            { "sent_to", tx.SentTo.Select(peer => peer.Peer) }
        };
        output.WriteOutput(result);
        output.WriteMessage($"Do '[grey]rchia wallet get-transaction -tx {tx.Name}[/]' to get status");
    }

    protected void FormatTransaction(TransactionRecord tx, string prefix, IDictionary<string, object?> row)
    {
        var name = Verbose || Json ? tx.Name : string.Concat(tx.Name.AsSpan(2, 10), "...");
        var status = tx.Confirmed
                        ? new Formattable<string>("Confirmed", "green")
                        : tx.IsInMempool
                        ? new Formattable<string>("In mempool", "orange3")
                        : new Formattable<string>("Pending", "grey");

        var amount = tx.Amount.ToChia();
        var bech32 = new Bech32M(prefix);
        var to = Verbose || Json ? bech32.PuzzleHashToAddress(tx.ToPuzzleHash) : string.Concat(bech32.PuzzleHashToAddress(tx.ToPuzzleHash).AsSpan(prefix.Length, 10), "...");
        var at = tx.CreatedAtDateTime.ToLocalTime();

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
            _ = await walletProxy.LogIn(Fingerprint.Value, cts.Token);
        }
        else
        {
            _ = await walletProxy.LogIn(cts.Token);
        }

        return walletProxy;
    }
}
