using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Bech32;
using rchia.Commands;
using Spectre.Console;

namespace rchia.Show
{
    internal class ShowTasks : ConsoleTask<FullNodeProxy>
    {
        public ShowTasks(FullNodeProxy fullNode, IConsoleMessage consoleMessage)
            : base(fullNode, consoleMessage)
        {
        }

        public async Task Prune(int age)
        {
            using var cts = new CancellationTokenSource(30000);

            var cutoff = DateTime.UtcNow - new TimeSpan(age, 0, 0);
            ConsoleMessage.MarkupLine($"Pruning connections that haven't sent a message since [wheat1]{cutoff.ToLocalTime()}[/]");

            var connections = await Service.GetConnections(cts.Token);
            var n = 0;
            // only prune other full nodes, not famers, harvesters, and wallets etc
            foreach (var connection in connections.Where(c => c.Type == NodeType.FULL_NODE && c.LastMessageDateTime < cutoff))
            {
                using var cts1 = new CancellationTokenSource(10000);
                await Service.CloseConnection(connection.NodeId, cts1.Token);
                ConsoleMessage.MarkupLine($"Closed connection at [wheat1]{connection.PeerHost}:{connection.PeerServerPort}[/] that last updated [wheat1]{connection.LastMessageDateTime.ToLocalTime()}[/]");
                n++;
            }

            ConsoleMessage.MarkupLine($"Pruned [wheat1]{n}[/] connection{(n == 1 ? string.Empty : "s")}");
        }

        public async Task AddConnection(string hostUri)
        {
            using var cts = new CancellationTokenSource(30000);
            var uri = hostUri.StartsWith("http") ? new Uri(hostUri) : new Uri("https://" + hostUri); // need to add a scheme so uri can be parsed
            await Service.OpenConnection(uri.Host, uri.Port, cts.Token);
        }

        public async Task BlockHeaderHashByHeight(uint height)
        {
            using var cts = new CancellationTokenSource(30000);
            var block = await Service.GetBlockRecordByHeight(height, cts.Token);
        }

        public async Task BlockByHeaderHash(string headerHash)
        {
            using var cts = new CancellationTokenSource(30000);
            var full_block = await Service.GetBlock(headerHash, cts.Token);
            var block = await Service.GetBlockRecord(headerHash, cts.Token);
            var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);
            var previous = await Service.GetBlockRecord(block.PrevHash, cts.Token);

            ConsoleMessage.NameValue("Block Height", block.Height);
            ConsoleMessage.NameValue("Header Hash", block.HeaderHash);

            var timestamp = block.DateTimestamp.HasValue ? block.DateTimestamp.Value.ToLocalTime().ToString() : "Not a transaction block";
            ConsoleMessage.NameValue("Timestamp", timestamp);
            ConsoleMessage.NameValue("Weight", block.Weight);
            ConsoleMessage.NameValue("Previous Block", block.PrevHash);

            var difficulty = previous is not null ? block.Weight - previous.Weight : block.Weight;
            ConsoleMessage.NameValue("Difficulty", difficulty);
            ConsoleMessage.NameValue("Sub-slot iters", block.SubSlotIters);
            ConsoleMessage.NameValue("Cost", full_block.TransactionsInfo?.Cost);
            ConsoleMessage.NameValue("Total VDF Iterations", block.TotalIters);
            ConsoleMessage.NameValue("Is a Transaction Block", full_block.RewardChainBlock.IsTransactionBlock);
            ConsoleMessage.NameValue("Deficit", block.Deficit);
            ConsoleMessage.NameValue("PoSpace 'k' Size", full_block.RewardChainBlock.ProofOfSpace.Size);
            ConsoleMessage.NameValue("Plot Public Key", full_block.RewardChainBlock.ProofOfSpace.PlotPublicKey);

            var poolPk = full_block.RewardChainBlock.ProofOfSpace.PublicPoolKey;
            poolPk = string.IsNullOrEmpty(poolPk) ? "Pay to pool puzzle hash" : poolPk;
            ConsoleMessage.NameValue("Pool Public Key", poolPk);
            ConsoleMessage.NameValue("Tx Filter Hash", full_block.RewardChainBlock.ProofOfSpace.PlotPublicKey);
            ConsoleMessage.NameValue("Plot Public Key", full_block.RewardChainBlock.ProofOfSpace.PlotPublicKey);

            var txFilterHash = full_block.FoliageTransactionBlock is not null ? full_block.FoliageTransactionBlock.FilterHash : "Not a transaction block";
            ConsoleMessage.NameValue("Tx Filter Hash", txFilterHash);

            var bech32 = new Bech32M(NetworkPrefix);
            var farmerAddress = bech32.PuzzleHashToAddress(block.FarmerPuzzleHash);
            var poolAddress = bech32.PuzzleHashToAddress(block.PoolPuzzleHash);

            ConsoleMessage.NameValue("Farmer Address", farmerAddress);
            ConsoleMessage.NameValue("Pool Address", poolAddress);

            var fees = block.Fees.HasValue ? block.Fees.Value.ToString() : "Not a transaction block";
            ConsoleMessage.NameValue("Fees Amount", fees);
        }

        public async Task Connections()
        {
            using var cts = new CancellationTokenSource(30000);
            var connections = await Service.GetConnections(cts.Token);

            ConsoleMessage.MarkupLine("[wheat1]Connections[/]");
            var table = new Table();
            table.AddColumn("Type");
            table.AddColumn("IP");
            table.AddColumn("Ports");
            table.AddColumn("NodeID");
            table.AddColumn("Last Connect");
            table.AddColumn("Up");
            table.AddColumn("Down");
            table.AddColumn("Height");
            table.AddColumn("Hash");

            foreach (var c in connections)
            {
                var id = ConsoleMessage.Verbose ? c.NodeId : c.NodeId.Substring(2, 10) + "...";
                var up = c.BytesRead.HasValue ? c.BytesRead.Value : 0;
                var down = c.BytesWritten.HasValue ? c.BytesWritten.Value : 0;
                var ports = $"{c.PeerPort}/{c.PeerServerPort}";
                var height = c.PeakHeight.HasValue ? c.PeakHeight.Value.ToString() : "na";
                var hash = string.IsNullOrEmpty(c.PeakHash) ? "no info" : ConsoleMessage.Verbose ? c.PeakHash : c.PeakHash.Substring(2, 10) + "...";

                table.AddRow(c.Type.ToString(), c.PeerHost, ports, id, $"{c.LastMessageDateTime.ToLocalTime():MMM dd HH:mm}", up.ToBytesString("N1"), down.ToBytesString("N1"), height, hash);
            }

            AnsiConsole.Render(table);
        }

        public async Task Exit()
        {
            using var cts = new CancellationTokenSource(30000);
            await Service.StopNode(cts.Token);
        }

        public async Task RemoveConnection(string nodeId)
        {
            using var cts = new CancellationTokenSource(30000);
            await Service.CloseConnection(nodeId, cts.Token);
        }

        public async Task State()
        {
            using var cts = new CancellationTokenSource(30000);
            var state = await Service.GetBlockchainState(cts.Token);
            var peakHash = state.Peak is not null ? state.Peak.HeaderHash : "";

            if (state.Sync.Synced)
            {
                ConsoleMessage.NameValue("Current Blockchain Status", "[green]Full Node Synced[/]");
                ConsoleMessage.NameValue("Peak Hash", peakHash);
            }
            else if (state.Peak is not null && state.Sync.SyncMode)
            {
                ConsoleMessage.NameValue("Current Blockchain Status", $"[yellow]Syncing[/] {state.Sync.SyncProgressHeight}/{state.Sync.SyncTipHeight}");
                ConsoleMessage.NameValue("Peak Hash", peakHash.Replace("0x", ""));
            }
            else if (state.Peak is not null)
            {
                ConsoleMessage.NameValue("Current Blockchain Status", $"[red]Not Synced[/] Peak height: {state.Peak.Height}");
            }
            else
            {
                ConsoleMessage.Warning("Searching for an initial chain");
                ConsoleMessage.MarkupLine("You may be able to expedite with '[grey]rchia show -a host:port[/]' using a known node.");
            }

            if (state.Peak is not null)
            {
                var time = state.Peak.DateTimestamp.HasValue ? state.Peak.DateTimestamp.Value.ToLocalTime().ToString("U") : "unknown";
                ConsoleMessage.MarkupLine($"      [wheat1]Time:[/] {time}\t\t[wheat1]Height[/]:\t{state.Peak.Height}");
            }

            ConsoleMessage.WriteLine("");
            ConsoleMessage.NameValue("Estimated network space", state.Space.ToBytesString());
            ConsoleMessage.NameValue("Current difficulty", state.Difficulty);
            ConsoleMessage.NameValue("Current VDF sub_slot_iters", state.SubSlotIters);

            var totalIters = state.Peak is not null ? state.Peak.TotalIters : 0;
            ConsoleMessage.NameValue("Total iterations since the start of the blockchain", totalIters);
            ConsoleMessage.WriteLine("");
            ConsoleMessage.MarkupLine("   [wheat1]Height | Hash[/]");

            if (state.Peak is not null)
            {
                var blocks = new List<BlockRecord>();

                var block = await Service.GetBlockRecord(state.Peak.HeaderHash, cts.Token);
                while (block is not null && blocks.Count < 10 && block.Height > 0)
                {
                    using var cts1 = new CancellationTokenSource(30000);
                    blocks.Add(block);
                    block = await Service.GetBlockRecord(block.PrevHash, cts.Token);
                }

                foreach (var b in blocks)
                {
                    ConsoleMessage.MarkupLine($"   {b.Height} | {b.HeaderHash.Replace("0x", "")}");
                }
            }
        }
    }
}
