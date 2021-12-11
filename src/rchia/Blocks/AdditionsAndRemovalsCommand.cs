using System;
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
        return await DoWorkAsync("Retrieving block header...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.FullNode);
            var proxy = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);
            var headerHash = await GetHash(proxy);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var (Additions, Removals) = await proxy.GetAdditionsAndRemovals(headerHash, cts.Token);

            if (Json)
            {
                var result = new Dictionary<string, IEnumerable<CoinRecord>>
                {
                    { "additions", Additions },
                    { "removals", Removals }
                };
                output.WriteOutput(result);
            }
            else
            {
                var result = new Dictionary<string, IEnumerable<IDictionary<string, object?>>>
                {
                    { "additions", GetCoinRecords(Additions) },
                    { "removals", GetCoinRecords(Removals) }
                };
                output.WriteOutput(result);
            }
        });
    }

    private static IEnumerable<IDictionary<string, object?>> GetCoinRecords(IEnumerable<CoinRecord> records)
    {
        var table = new List<IDictionary<string, object?>>();

        foreach (var record in records)
        {
            var row = new Dictionary<string, object?>
            {
                { "parent_coin_info", record.Coin.ParentCoinInfo.Replace("0x", "") },
                { "puzzle_hash", record.Coin.PuzzleHash.Replace("0x", "") },
                { "amount", record.Coin.Amount.ToChia() },
                { "confirmed_at", record.ConfirmedBlockIndex },
                { "spent_at", record.SpentBlockIndex },
                { "spent", record.Spent },
                { "coinbase", record.Coinbase },
                { "timestamp", record.DateTimestamp.ToLocalTime() }
            };

            table.Add(row);
        }

        return table;
    }
}
