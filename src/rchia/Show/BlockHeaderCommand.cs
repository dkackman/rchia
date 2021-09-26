using System;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Bech32;
using rchia.Commands;

namespace rchia.Show
{
    internal sealed class BlockHeaderCommand : EndpointOptions
    {
        [Option("a", "hash", ArgumentHelpName = "HASH", Description = "Look up a block by header hash")]
        public string? Hash { get; init; }

        [Option("h", "height", ArgumentHelpName = "HEIGHT", Description = "Look up a block by height")]
        public uint? Height { get; init; }

        private async Task<string> GetHash(FullNodeProxy proxy)
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            if (Height.HasValue)
            {
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
        public async override Task<int> Run()
        {
            return await DoWork2("Retrieving block header connection...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.FullNode);
                var proxy = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);
                var headerHash = await GetHash(proxy);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var full_block = await proxy.GetBlock(headerHash, cts.Token);
                var block = await proxy.GetBlockRecord(headerHash, cts.Token);

                var (NetworkName, NetworkPrefix) = await proxy.GetNetworkInfo(cts.Token);
                var previous = await proxy.GetBlockRecord(block.PrevHash, cts.Token);

                NameValue("Block Height", block.Height);
                NameValue("Header Hash", block.HeaderHash);

                var timestamp = block.DateTimestamp.HasValue ? block.DateTimestamp.Value.ToLocalTime().ToString() : "Not a transaction block";
                NameValue("Timestamp", timestamp);
                NameValue("Weight", block.Weight);
                NameValue("Previous Block", block.PrevHash);

                var difficulty = previous is not null ? block.Weight - previous.Weight : block.Weight;
                NameValue("Difficulty", difficulty);
                NameValue("Sub-slot iters", block.SubSlotIters.ToString("N0"));
                NameValue("Cost", full_block.TransactionsInfo?.Cost);
                NameValue("Total VDF Iterations", block.TotalIters.ToString("N0"));
                NameValue("Is a Transaction Block", full_block.RewardChainBlock.IsTransactionBlock);
                NameValue("Deficit", block.Deficit);
                NameValue("PoSpace 'k' Size", full_block.RewardChainBlock.ProofOfSpace.Size);
                NameValue("Plot Public Key", full_block.RewardChainBlock.ProofOfSpace.PlotPublicKey);

                var poolPk = full_block.RewardChainBlock.ProofOfSpace.PublicPoolKey;
                poolPk = string.IsNullOrEmpty(poolPk) ? "Pay to pool puzzle hash" : poolPk;
                NameValue("Pool Public Key", poolPk);
                NameValue("Tx Filter Hash", full_block.RewardChainBlock.ProofOfSpace.PlotPublicKey);
                NameValue("Plot Public Key", full_block.RewardChainBlock.ProofOfSpace.PlotPublicKey);

                var txFilterHash = full_block.FoliageTransactionBlock is not null ? full_block.FoliageTransactionBlock.FilterHash : "Not a transaction block";
                NameValue("Tx Filter Hash", txFilterHash);

                var bech32 = new Bech32M(NetworkPrefix);
                var farmerAddress = bech32.PuzzleHashToAddress(block.FarmerPuzzleHash);
                var poolAddress = bech32.PuzzleHashToAddress(block.PoolPuzzleHash);

                NameValue("Farmer Address", farmerAddress);
                NameValue("Pool Address", poolAddress);

                var fees = block.Fees.HasValue ? block.Fees.Value.ToString() : "Not a transaction block";
                NameValue("Fees Amount", fees);
            });
        }
    }
}
