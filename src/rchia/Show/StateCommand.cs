using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using Spectre.Console;

namespace rchia.Show
{
    internal sealed class StateCommand : EndpointOptions
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            return await DoWork2("Retrieving node info...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.FullNode);
                var proxy = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var state = await proxy.GetBlockchainState(cts.Token);
                var peakHash = state.Peak is not null ? state.Peak.HeaderHash : "";

                if (state.Sync.Synced)
                {
                    NameValue("Current Blockchain Status", "[green]Full Node Synced[/]");
                    NameValue("Peak Hash", peakHash);
                }
                else if (state.Peak is not null && state.Sync.SyncMode)
                {
                    NameValue("Current Blockchain Status", $"[yellow]Syncing[/] {state.Sync.SyncProgressHeight}/{state.Sync.SyncTipHeight}");
                    NameValue("Peak Hash", peakHash.Replace("0x", ""));
                }
                else if (state.Peak is not null)
                {
                    NameValue("Current Blockchain Status", $"[red]Not Synced[/] Peak height: {state.Peak.Height}");
                }
                else
                {
                    Warning("Searching for an initial chain");
                    MarkupLine("You may be able to expedite with '[grey]rchia show -a host:port[/]' using a known node.");
                }

                if (state.Peak is not null)
                {
                    var time = state.Peak.DateTimestamp.HasValue ? state.Peak.DateTimestamp.Value.ToLocalTime().ToString("U") : "unknown";
                    NameValue("Time", time);
                    NameValue("Peak Height", state.Peak.Height);
                }

                WriteLine("");
                NameValue("Estimated network space", state.Space.ToBytesString());
                NameValue("Current difficulty", state.Difficulty);
                NameValue("Current VDF sub_slot_iters", state.SubSlotIters.ToString("N0"));

                var totalIters = state.Peak is not null ? state.Peak.TotalIters : 0;
                NameValue("Total iterations since the start of the blockchain", totalIters.ToString("N0"));

                if (state.Peak is not null)
                {
                    var blocks = new List<BlockRecord>();

                    var block = await proxy.GetBlockRecord(state.Peak.HeaderHash, cts.Token);
                    while (block is not null && blocks.Count < 10 && block.Height > 0)
                    {
                        using var cts1 = new CancellationTokenSource(TimeoutMilliseconds);
                        blocks.Add(block);
                        block = await proxy.GetBlockRecord(block.PrevHash, cts.Token);
                    }

                    var table = new Table();
                    table.AddColumn("[orange3]Height[/]");
                    table.AddColumn("[orange3]Hash[/]");

                    foreach (var b in blocks)
                    {
                        table.AddRow(b.Height.ToString(), b.HeaderHash.Replace("0x", ""));
                    }
                    AnsiConsole.Render(table);
                }
            });
        }
    }
}
