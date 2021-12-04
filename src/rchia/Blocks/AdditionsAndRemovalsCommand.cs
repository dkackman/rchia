using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Blocks;

internal sealed class AdditionsAndRemovalsCommand : EndpointOptions
{
    [Option("a", "hash", ArgumentHelpName = "HASH", Description = "Look up by header hash")]
    public string? Hash { get; init; }

    [Option("h", "height", ArgumentHelpName = "HEIGHT", Description = "Look up by height")]
    public uint? Height { get; init; }

    private async Task<string> GetHash(FullNodeProxy proxy)
    {
        if (Height.HasValue)
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var block = await proxy.GetBlockRecordByHeight(Height.Value, cts.Token);

            return block.HeaderHash;
        }

        if (!string.IsNullOrEmpty(Hash))
        {
            return Hash;
        }

        throw new InvalidOperationException("Either a valid block height or header hash must be specified.");
    }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving block header connection...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.FullNode);
            var proxy = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);
            var headerHash = await GetHash(proxy);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var (Additions, Removals) = await proxy.GetAdditionsAndRemovals(headerHash, cts.Token);

            ShowCoinRecords(output, Additions, "Additions");
            ShowCoinRecords(output, Removals, "Removals");
        });
    }

    private void ShowCoinRecords(ICommandOutput output, IEnumerable<CoinRecord> records, string name)
    {
        var table = new List<IDictionary<string, string>>();

        foreach (var record in records)
        {
            var row = new Dictionary<string, string>();

            row.Add("parent_coin_info", record.Coin.ParentCoinInfo.Replace("0x", ""));
            row.Add("puzzle_hash", record.Coin.PuzzleHash.Replace("0x", ""));
            row.Add($"amount", record.Coin.Amount.AsChia("N3"));
            row.Add("confirmed_at", record.ConfirmedBlockIndex.ToString("N0"));
            row.Add("spent_at", record.SpentBlockIndex.ToString("N0"));
            row.Add("spen", record.Spent.ToString());
            row.Add("coinbase", record.Coinbase.ToString());
            row.Add("timestamp", record.DateTimestamp.ToLocalTime().ToString());

            table.Add(row);
        }

        output.WriteOutput(table);

        output.MarkupLine($"{records.Count()} {(records.Count() == 1 ? name.ToLower().TrimEnd('s') : name.ToLower())}");
    }
}
