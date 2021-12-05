﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using Spectre.Console;

namespace rchia.Node;

internal sealed class StatusCommand : EndpointOptions
{
    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving node info...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.FullNode);
            var proxy = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var state = await proxy.GetBlockchainState(cts.Token);
            var peakHash = state.Peak is not null ? state.Peak.HeaderHash : string.Empty;

            if (state.Sync.Synced)
            {
                NameValue("Current Blockchain Status", "[green]Full Node Synced[/]");
                NameValue("Peak Hash", peakHash.Replace("0x", ""));
            }
            else if (state.Peak is not null && state.Sync.SyncMode)
            {
                NameValue("Current Blockchain Status", $"[yellow]Syncing[/] {state.Sync.SyncProgressHeight:N0} of {state.Sync.SyncTipHeight:N0}");
                NameValue("Blocks behind", $"{(state.Sync.SyncTipHeight - state.Sync.SyncProgressHeight):N0}");
                NameValue("Peak Hash", peakHash.Replace("0x", string.Empty));
            }
            else if (state.Peak is not null)
            {
                NameValue("Current Blockchain Status", $"[red]Not Synced[/] Peak height: {state.Peak.Height}");
            }
            else
            {
                output.Warning("Searching for an initial chain");
                output.MarkupLine("You may be able to expedite with '[grey]rchia show add host:port[/]' using a known node.");
            }

            if (state.Peak is not null)
            {
                var peak_time = state.Peak.DateTimestamp;
                if (!state.Peak.IsTransactionBlock)
                {
                    var curr = await proxy.GetBlockRecord(state.Peak.HeaderHash, cts.Token);

                    while (curr is not null && !curr.IsTransactionBlock)
                    {
                        curr = await proxy.GetBlockRecord(curr.PrevHash, cts.Token);
                    }

                    peak_time = curr?.DateTimestamp;
                }

                var time = peak_time.HasValue ? peak_time.Value.ToLocalTime().ToString("U") : "unknown";
                NameValue("Time", time);
                NameValue("Peak Height", state.Peak.Height.ToString("N0"));
            }

            NameValue("Estimated network space", state.Space.ToBytesString());
            NameValue("Current difficulty", state.Difficulty);
            NameValue("Current VDF sub_slot_iters", state.SubSlotIters.ToString("N0"));

            var totalIters = state.Peak is not null ? state.Peak.TotalIters : 0;
            NameValue("Total iterations since the start of the blockchain", totalIters.ToString("N0"));

            if (Verbose && state.Peak is not null)
            {
                var blocks = new List<BlockRecord>();
                var block = await proxy.GetBlockRecord(state.Peak.HeaderHash, cts.Token);

                while (block is not null && blocks.Count < 10 && block.Height > 0)
                {
                    blocks.Add(block);
                    using var cts1 = new CancellationTokenSource(TimeoutMilliseconds);
                    block = await proxy.GetBlockRecord(block.PrevHash, cts.Token);
                }

                var table = new Table
                {
                    Title = new TableTitle("[orange3]Recent Blocks[/]")
                };

                table.AddColumn("[orange3]Height[/]");
                table.AddColumn("[orange3]Hash[/]");

                foreach (var b in blocks)
                {
                    table.AddRow(b.Height.ToString("N0"), b.HeaderHash.Replace("0x", ""));
                }

                AnsiConsole.Write(table);
            }
        });
    }
}
