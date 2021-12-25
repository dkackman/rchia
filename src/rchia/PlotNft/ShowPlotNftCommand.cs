using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using chia.dotnet;
using rchia.Bech32;
using rchia.Commands;

namespace rchia.PlotNft;

internal sealed class ShowPlotNftCommand : WalletCommand
{
    [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
    public uint Id { get; init; } = 1;

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving pool info...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(output, this);
            var daemon = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
            var farmer = daemon.CreateProxyFrom<FarmerProxy>();
            var wallet = daemon.CreateProxyFrom<WalletProxy>();

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var allWallets = await wallet.GetWallets(cts.Token);
            var poolingWallets = allWallets.Where(w => w.Type == WalletType.POOLING_WALLET);
            if (!poolingWallets.Any())
            {
                throw new InvalidOperationException("There are no pooling wallets on this node.");
            }

            // since the command specified a specific wallet narrow the result to just that
            var walletInfo = poolingWallets.FirstOrDefault(w => w.Id == Id);
            if (walletInfo is null)
            {
                throw new InvalidOperationException($"Wallet with id: {Id} is not a pooling wallet or doesn't exist. Please provide a different id.");
            }

            var poolStates = await farmer.GetPoolState(cts.Token);
            var harvesters = await farmer.GetHarvesters(cts.Token);
            var (NetworkName, NetworkPrefix) = await wallet.GetNetworkInfo(cts.Token);
            var height = await wallet.GetHeightInfo(cts.Token);
            var (GenesisInitialized, Synced, Syncing) = await wallet.GetSyncStatus(cts.Token);

            var result = new Dictionary<string, object?>()
            {
                { "wallet_height", height },
                { "sync_status", $"{(Synced ? string.Empty : "not ")}synced"},
                { "wallet", walletInfo.Id }
            };

            var poolwallet = new PoolWallet(walletInfo.Id, wallet);
            using var cts1 = new CancellationTokenSource(TimeoutMilliseconds);
            var (State, UnconfirmedTransactions) = await poolwallet.Status(cts1.Token);

            if (State.Current.State == PoolSingletonState.LEAVING_POOL)
            {
                var expected = State.SingletonBlockHeight - State.Current.RelativeLockHeight;
                output.WriteWarning($"Current state: INVALID_STATE. Please leave/join again after block height {expected}");
            }
            else
            {
                result.Add($"current_state", State.Current.State);
            }

            var bech32 = new Bech32M(NetworkPrefix);
            result.Add($"current_state_from_block_height", State.SingletonBlockHeight);
            result.Add($"launcher_id", State.LauncherId);
            result.Add($"target_address", bech32.PuzzleHashToAddress(State.Current.TargetPuzzleHash));

            var poolPlots = from h in harvesters
                            from plots in h.Plots
                            where plots.PoolContractPuzzleHash == State.P2SingletonPuzzleHash
                            select plots;

            var plotCount = poolPlots.Count();
            result.Add($"number_of_plots", plotCount);
            result.Add($"owner_public_key", State.Current.OwnerPubkey);
            result.Add($"pool_contract_address", $"{bech32.PuzzleHashToAddress(State.P2SingletonPuzzleHash)}");

           // MarkupLine($"Pool contract address (use ONLY for plotting - do not send money to this address) {bech32.PuzzleHashToAddress(State.P2SingletonPuzzleHash)}");

            if (State.Target is not null)
            {
                result.Add($"target_state", State.Target.State);
                result.Add($"target_pool_url", State.Target.PoolUrl);
            }

            if (State.Current.State == PoolSingletonState.SELF_POOLING)
            {
                var (ConfirmedWalletBalance, UnconfirmedWalletBalance, SpendableBalance, PendingChange, MaxSendAmount, UnspentCoinCount, PendingCoinRemovalCount) = await poolwallet.GetBalance(cts1.Token);
                result.Add($"claimable_balance", $"{ConfirmedWalletBalance.ToChia()} {NetworkPrefix}");
            }
            else if (State.Current.State == PoolSingletonState.FARMING_TO_POOL)
            {
                result.Add($"current_pool_url", State.Current.PoolUrl ?? string.Empty);
                var poolstate = poolStates.FirstOrDefault(ps => ps.PoolConfig.LauncherId == State.LauncherId);
                if (poolstate is not null)
                {
                    result.Add($"current_difficulty", poolstate.CurrentDifficulty);
                    result.Add($"points_balance", poolstate.CurrentPoints);
                    if (poolstate.PointsFound24h.Any())
                    {
                        var pointsAcknowledged = poolstate.PointsAcknowledged24h.Count;
                        var pct = pointsAcknowledged / (double)poolstate.PointsFound24h.Count;
                        result.Add($"percent_successful_points_24h", $"{pct:P2}%");
                    }
                    result.Add($"relative_lock_height", State.Current.RelativeLockHeight);
                    try
                    {
                        result.Add("payout_instructions", bech32.PuzzleHashToAddress(poolstate.PoolConfig.PayoutInstructions));

                       // MarkupLine($"Payout instructions (pool will pay to this address) [green]{bech32.PuzzleHashToAddress(poolstate.PoolConfig.PayoutInstructions)}[/]");
                    }
                    catch
                    {
                        result.Add("payout_instructions", poolstate.PoolConfig.PayoutInstructions);

                        //MarkupLine($"Payout instructions (pool will pay you with this) {poolstate.PoolConfig.PayoutInstructions}");
                    }
                }
            }
            else if (State.Current.State == PoolSingletonState.LEAVING_POOL)
            {
                var expected = State.SingletonBlockHeight - State.Current.RelativeLockHeight;
                if (State.Target is not null)
                {
                    result.Add("predicted_leave_after_block_height", expected);
                }
            }

            output.WriteOutput(result);
        });
    }
}
