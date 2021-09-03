using System;
using System.Threading;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;

namespace rchia.Netspace
{
    internal sealed class NetspaceTasks : ConsoleTask<FullNodeProxy>
    {
        public NetspaceTasks(FullNodeProxy fullNodeProxy, IConsoleMessage consoleMessage)
            : base(fullNodeProxy, consoleMessage)
        {
        }

        public async Task Netspace(string? start, uint delta)
        {
            using var cts = new CancellationTokenSource(200000);

            uint newer_block_height = 0;
            if (string.IsNullOrEmpty(start))
            {
                var blockchain_state = await Service.GetBlockchainState(cts.Token);
                if (blockchain_state.Peak is null)
                {
                    Console.WriteLine("No blocks in blockchain");
                    return;
                }

                newer_block_height = blockchain_state.Peak.Height;
            }
            else
            {
                var newer_block = await Service.GetBlockRecord(start, cts.Token);
                if (newer_block is null)
                {
                    Console.WriteLine($"Block header hash {start} not found.");
                    return;
                }

                newer_block_height = newer_block.Height;
            }

            Console.WriteLine($"newer_height      {newer_block_height}");

            var newer_block_header = await Service.GetBlockRecordByHeight(newer_block_height);
            var older_block_height = Math.Max(0, newer_block_height - delta);
            var older_block_header = await Service.GetBlockRecordByHeight(older_block_height);
            var network_space_bytes_estimate = await Service.GetNetworkSpace(newer_block_header.HeaderHash, older_block_header.HeaderHash);

            Console.WriteLine("Older Block\n" +
                $"Block Height:     {older_block_header.Height}\n" +
                $"Weight:           {older_block_header.Weight}\n" +
                $"VDF Iterations:   {older_block_header.TotalIters}\n" +
                $"Header Hash:      0x{older_block_header.HeaderHash}\n");
            Console.WriteLine("Newer Block\n" +
                $"Block Height:     {newer_block_header.Height}\n" +
                $"Weight:           {newer_block_header.Weight}\n" +
                $"VDF Iterations:   {newer_block_header.TotalIters}\n" +
                $"Header Hash:      0x{newer_block_header.HeaderHash}\n");

            Console.WriteLine(network_space_bytes_estimate.ToBytesString());
        }
    }
}
