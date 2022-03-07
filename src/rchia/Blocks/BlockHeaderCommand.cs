using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using chia.dotnet.bech32;
using rchia.Commands;

namespace rchia.Blocks;

internal sealed class BlockHeaderCommand : EndpointOptions
{
    [Option("a", "hash", ArgumentHelpName = "HASH", Description = "Look up a block by header hash")]
    public string? Hash { get; init; }

    [Option("h", "height", ArgumentHelpName = "HEIGHT", Description = "Look up a block by height")]
    public int? Height { get; init; }

    private async Task<string> GetHash(FullNodeProxy proxy)
    { 
        if (Height.HasValue)
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var block = await proxy.GetBlockRecordByHeight((uint)Height.Value, cts.Token);

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
            var full_block = await proxy.GetBlock(headerHash, cts.Token);
            var block = await proxy.GetBlockRecord(headerHash, cts.Token);

            if (Json)
            {
                var dict = new Dictionary<string, object?>()
                {
                    { "block", block },
                    { "full_block", full_block },

                };
                output.WriteOutput(dict);
            }
            else
            {
                var (NetworkName, NetworkPrefix) = await proxy.GetNetworkInfo(cts.Token);
                var previous = await proxy.GetBlockRecord(block.PrevHash, cts.Token);

                var result = new Dictionary<string, object?>
                {
                    { "block_height", block.Height },
                    { "header_hash", new Formattable<string>(block.HeaderHash, hash => hash.Replace("0x", string.Empty)) }
                };

                var timestamp = block.DateTimestamp.HasValue ? block.DateTimestamp.Value.ToLocalTime().ToString() : "Not a transaction block";
                result.Add("timestamp", timestamp);
                result.Add("weight", block.Weight);
                result.Add("previous_block", new Formattable<string>(block.PrevHash, hash => hash.Replace("0x", string.Empty)));

                var difficulty = previous is not null ? block.Weight - previous.Weight : block.Weight;
                result.Add("difficulty", difficulty);
                result.Add("subslot_iters", block.SubSlotIters);
                result.Add("cost", full_block.TransactionsInfo?.Cost);
                result.Add("total_vdf_iterations", block.TotalIters);
                result.Add("is_transaction_block", full_block.RewardChainBlock.IsTransactionBlock);
                result.Add("deficit", block.Deficit);
                result.Add("k_size", full_block.RewardChainBlock.ProofOfSpace.Size);
                result.Add("plot_public_key", full_block.RewardChainBlock.ProofOfSpace.PlotPublicKey);

                var poolPk = full_block.RewardChainBlock.ProofOfSpace.PublicPoolKey;
                result.Add("pool_public_key", string.IsNullOrEmpty(poolPk) ? "Pay to pool puzzle hash" : poolPk);

                var txFilterHash = full_block.FoliageTransactionBlock is not null ? full_block.FoliageTransactionBlock.FilterHash : "Not a transaction block";
                result.Add("tx_filter_hash", new Formattable<string>(txFilterHash, hash => hash.Replace("0x", string.Empty)));

                var bech32 = new Bech32M(NetworkPrefix);
                var farmerAddress = bech32.PuzzleHashToAddress(block.FarmerPuzzleHash.Replace("0x", string.Empty));
                var poolAddress = bech32.PuzzleHashToAddress(block.PoolPuzzleHash.Replace("0x", string.Empty));

                result.Add("farmer_address", farmerAddress);
                result.Add("pool_address", poolAddress);

                var fees = block.Fees.HasValue ? block.Fees.Value.ToString() : "Not a transaction block";
                result.Add("fees_amount", fees);

                output.WriteOutput(result);
            }
        });
    }
}
