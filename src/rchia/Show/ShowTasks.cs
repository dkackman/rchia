using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using chia.dotnet;
using chia.dotnet.console;

using rchia.Bech32;

namespace rchia.Show
{
    internal class ShowTasks : ConsoleTask<FullNodeProxy>
    {
        public ShowTasks(FullNodeProxy fullNode, bool verbose, IConsoleMessage consoleMessage)
            : base(fullNode, consoleMessage)
        {
            Verbose = verbose;
        }

        public bool Verbose { get; init; }

        public async Task AddConnection(string hostUri)
        {
            ConsoleMessage.Message($"Adding {hostUri}...");

            using var cts = new CancellationTokenSource(5000);
            var uri = new Uri("https://" + hostUri); // need to add a scheme so uri can be parsed
            await Service.OpenConnection(uri.Host, uri.Port, cts.Token);

            ConsoleMessage.Message($"Successfully added {hostUri}.");
        }

        public async Task BlockByHeaderHash(string headerHash)
        {
            ConsoleMessage.Message("Retrieving block {headerHash}...");

            using var cts = new CancellationTokenSource(5000);
            var full_block = await Service.GetBlock(headerHash, cts.Token);
            var block = await Service.GetBlockRecord(headerHash, cts.Token);
            var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);
            var previous = await Service.GetBlockRecord(block.PrevHash, cts.Token);

            ConsoleMessage.Message("Done.");


            Console.WriteLine($"Block Height           {block.Height}");
            Console.WriteLine($"Header Hash            {block.HeaderHash}");

            var timestamp = block.DateTimestamp.HasValue ? block.DateTimestamp.Value.ToLocalTime().ToString() : "Not a transaction block";
            Console.WriteLine($"Timestamp              {timestamp}");
            Console.WriteLine($"Weight                 {block.Weight}");
            Console.WriteLine($"Previous Block         {block.PrevHash}");

            var difficulty = previous is not null ? block.Weight - previous.Weight : block.Weight;
            Console.WriteLine($"Difficulty             {difficulty}");
            Console.WriteLine($"Sub-slot iters         {block.SubSlotIters}");
            Console.WriteLine($"Cost                   {full_block.TransactionsInfo?.Cost}");
            Console.WriteLine($"Total VDF Iterations   {block.TotalIters}");
            Console.WriteLine($"Is a Transaction Block {full_block.RewardChainBlock.IsTransactionBlock}");
            Console.WriteLine($"Deficit                {block.Deficit}");
            Console.WriteLine($"PoSpace 'k' Size       {full_block.RewardChainBlock.ProofOfSpace.Size}");
            Console.WriteLine($"Plot Public Key        {full_block.RewardChainBlock.ProofOfSpace.PlotPublicKey}");

            var poolPk = full_block.RewardChainBlock.ProofOfSpace.PublicPoolKey;
            poolPk = string.IsNullOrEmpty(poolPk) ? "Pay to pool puzzle hash" : poolPk;
            Console.WriteLine($"Pool Public Key        {poolPk}");
            Console.WriteLine($"Tx Filter Hash         {full_block.RewardChainBlock.ProofOfSpace.PlotPublicKey}");
            Console.WriteLine($"Plot Public Key        {full_block.RewardChainBlock.ProofOfSpace.PlotPublicKey}");

            var txFilterHash = full_block.FoliageTransactionBlock is not null ? full_block.FoliageTransactionBlock.FilterHash : "Not a transaction block";
            Console.WriteLine($"Tx Filter Hash         {txFilterHash}");

            Bech32M.AddressPrefix = NetworkPrefix;

            var farmerAddress = Bech32M.PuzzleHashToAddress(HexBytes.FromHex(block.FarmerPuzzleHash));
            var poolAddress = Bech32M.PuzzleHashToAddress(HexBytes.FromHex(block.PoolPuzzleHash));
            Console.WriteLine($"Farmer Address         {farmerAddress}");
            Console.WriteLine($"Pool Address           {poolAddress}");

            var fees = block.Fees.HasValue ? block.Fees.Value.ToString() : "Not a transaction block";
            Console.WriteLine($"Fees Amount            {fees}");
        }

        public async Task Connections()
        {
            using var cts = new CancellationTokenSource(5000);
            var connections = await Service.GetConnections(cts.Token);

            Console.WriteLine("Connections:");
            var padding = Verbose ? "                                                             " : "        ";
            Console.WriteLine($"Type      IP                   Ports       NodeID{padding}Last Connect       MiB Up|Dwn");

            foreach (var c in connections)
            {
                Console.Write($"{c.Type,-9} {c.PeerHost,-15}      {c.PeerPort,5}/{c.PeerServerPort,-5} ");

                var id = Verbose ? c.NodeId : c.NodeId.Substring(2, 10) + "...";
                Console.Write($"{id} ");
                Console.Write($"{c.LastMessageDateTime.ToLocalTime():MMM dd HH:mm:ss}   ");

                var down = c.BytesRead / (1024 * 1024);
                var up = c.BytesWritten / (1024 * 1024);
                Console.Write($"{up,7:N1}|{down,-7:N1}");

                if (c.Type == NodeType.FULL_NODE)
                {
                    Console.WriteLine("");
                    var height = c.PeakHeight.HasValue ? c.PeakHeight : 0;
                    var hash = string.IsNullOrEmpty(c.PeakHash) ? "no info" :
                        Verbose ? c.PeakHash : c.PeakHash.Substring(2, 10) + "...";

                    Console.Write("                               ");
                    Console.Write($"-SB Height: {height,8}    -Hash: {hash}");
                }

                Console.WriteLine("");
            }
        }

        public async Task Exit()
        {
            ConsoleMessage.Message("Stopping the full node...");

            using var cts = new CancellationTokenSource(5000);
            await Service.StopNode(cts.Token);
        }

        public async Task RemoveConnection(string nodeId)
        {
            ConsoleMessage.Message($"Removing {nodeId}...");

            using var cts = new CancellationTokenSource(5000);
            await Service.CloseConnection(nodeId, cts.Token);

            ConsoleMessage.Message($"Removed {nodeId}.");
        }

        public async Task State()
        {
            using var cts = new CancellationTokenSource(5000);
            var state = await Service.GetBlockchainState(cts.Token);
            var peakHash = state.Peak is not null ? state.Peak.HeaderHash : "";

            if (state.Sync.Synced)
            {
                Console.WriteLine("Current Blockchain Status: Full Node Synced");
                Console.WriteLine($"Peak: Hash:{peakHash}");
            }
            else if (state.Peak is not null && state.Sync.SyncMode)
            {
                Console.WriteLine($"Current Blockchain Status: Syncing {state.Sync.SyncProgressHeight}/{state.Sync.SyncTipHeight}.");
                Console.WriteLine($"Peak: Hash: {peakHash.Replace("0x", "")}");
            }
            else if (state.Peak is not null)
            {
                Console.WriteLine($"Current Blockchain Status: Not Synced. Peak height: {state.Peak.Height}");
            }
            else
            {
                Console.WriteLine("Searching for an initial chain");
                Console.WriteLine("You may be able to expedite with 'chia show -a host:port' using a known node.");
            }

            if (state.Peak is not null)
            {
                var time = state.Peak.DateTimestamp.HasValue ? state.Peak.DateTimestamp.Value.ToLocalTime().ToString("U") : "unknown";
                Console.WriteLine($"      Time: {time}\t\tHeight:\t{state.Peak.Height}");
            }

            Console.WriteLine("");
            Console.WriteLine($"Estimated network space: {state.Space.ToBytesString()}");
            Console.WriteLine($"Current difficulty: {state.Difficulty}");
            Console.WriteLine($"Current VDF sub_slot_iters: {state.SubSlotIters}");

            var totalIters = state.Peak is not null ? state.Peak.TotalIters : 0;
            Console.WriteLine($"Total iterations since the start of the blockchain: {totalIters}");
            Console.WriteLine("");
            Console.WriteLine("  Height: | Hash:");

            if (state.Peak is not null)
            {
                var blocks = new List<BlockRecord>();

                var block = await Service.GetBlockRecord(state.Peak.HeaderHash, cts.Token);
                while (block is not null && blocks.Count < 10 && block.Height > 0)
                {
                    using var cts1 = new CancellationTokenSource(1000);
                    blocks.Add(block);
                    block = await Service.GetBlockRecord(block.PrevHash, cts.Token);
                }

                foreach (var b in blocks)
                {
                    Console.WriteLine($"   {b.Height} | {b.HeaderHash.Replace("0x", "")}");
                }
            }

        }
    }
}
