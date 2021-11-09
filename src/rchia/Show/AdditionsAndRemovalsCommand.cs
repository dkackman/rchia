using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using chia.dotnet;
using rchia.Commands;
using Spectre.Console;

namespace rchia.Show
{
    internal sealed class AdditionsAndRemovalsCommand : EndpointOptions
    {
        [Option("a", "hash", ArgumentHelpName = "HASH", Description = "Look up a block by header hash")]
        public string? Hash { get; init; }

        [Option("h", "height", ArgumentHelpName = "HEIGHT", Description = "Look up a block by height")]
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
            return await DoWorkAsync("Retrieving block header connection...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.FullNode);
                var proxy = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);
                var headerHash = await GetHash(proxy);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var (Additions, Removals) = await proxy.GetAdditionsAndRemovals(headerHash, cts.Token);
                var (NetworkName, NetworkPrefix) = await proxy.GetNetworkInfo(cts.Token);

                ShowCoinRecords(Additions, "Additions", NetworkPrefix);
                ShowCoinRecords(Removals, "Removals", NetworkPrefix);
            });
        }

        private void ShowCoinRecords(ICollection<CoinRecord> records, string name,string NetworkPrefix)
        {
            if (records.Any())
            {
                var table = new Table
                {
                    Title = new TableTitle($"[orange3]{name}[/]")
                };

                table.AddColumn("[orange3]ParentCoinInfo[/]");
                table.AddColumn("[orange3]PuzzleHash[/]");
                table.AddColumn($"[orange3]Amount ({NetworkPrefix})[/]");
                table.AddColumn("[orange3]Confirmed At[/]");
                table.AddColumn("[orange3]Spent At[/]");
                table.AddColumn("[orange3]Spent[/]");
                table.AddColumn("[orange3]Coinbase[/]");
                table.AddColumn("[orange3]Timestamp[/]");

                foreach (var record in records)
                {
                    table.AddRow(
                        record.Coin.ParentCoinInfo.Replace("0x", ""),
                        record.Coin.PuzzleHash.Replace("0x", ""),
                        record.Coin.Amount.ToChia().ToString("N3"),
                        record.ConfirmedBlockIndex.ToString("N0"),
                        record.SpentBlockIndex.ToString("N0"),
                        record.Spent.ToString(),
                        record.Coinbase.ToString(),
                        record.DateTimestamp.ToLocalTime().ToString()
                        );
                }

                AnsiConsole.Write(table);
            }

            MarkupLine($"{records.Count} {(records.Count == 1 ? name.ToLower().TrimEnd('s') : name.ToLower())}");
        }
    }
}
