﻿using System;
using System.Threading;
using System.Threading.Tasks;

using chia.dotnet;

namespace rchia.Show
{
    internal static class BlockByHeaderHashTask
    {
        public static async Task Run(FullNodeProxy fullNode, string headerHash, bool verbose)
        {
            if (verbose)
            {
                Console.WriteLine("Retrieving block...");
            }

            using var cts = new CancellationTokenSource(5000);
            var full_block = await fullNode.GetBlock(headerHash, cts.Token);
            var block = await fullNode.GetBlockRecord(headerHash, cts.Token);
            var info = await fullNode.GetNetworkInfo(cts.Token);
            var previous = await fullNode.GetBlockRecord(block.PrevHash, cts.Token);
            
            if (verbose)
            {
                Console.WriteLine("Done.");
            }

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

            Bech32M.AddressPrefix = info.NetworkPrefix;

            var farmerAddress = Bech32M.PuzzleHashToAddress(HexBytes.FromHex(block.FarmerPuzzleHash));
            var poolAddress = Bech32M.PuzzleHashToAddress(HexBytes.FromHex(block.PoolPuzzleHash));
            Console.WriteLine($"Farmer Address         {farmerAddress}");
            Console.WriteLine($"Pool Address           {poolAddress}");

            var fees = block.Fees.HasValue ? block.Fees.Value.ToString() : "Not a transaction block";
            Console.WriteLine($"Fees Amount            {fees}");
        }
    }
}