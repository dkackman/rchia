using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Bech32;
using rchia.Commands;

namespace rchia.PlotNft
{
    internal sealed class ShowPlotNftCommand : WalletCommand
    {
        [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
        public uint Id { get; init; } = 1;

        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Retrieving pool info...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(ctx, this, ServiceNames.Daemon);
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

                NameValue($"Wallet height", height);
                NameValue($"Sync status", $"{(Synced ? "" : "not ")}synced");

                NameValue($"Wallet", walletInfo.Id);
                var poolwallet = new PoolWallet(walletInfo.Id, wallet);
                using var cts1 = new CancellationTokenSource(TimeoutMilliseconds);
                var (State, UnconfirmedTransactions) = await poolwallet.Status(cts1.Token);

                if (State.Current.State == PoolSingletonState.LEAVING_POOL)
                {
                    var expected = State.SingletonBlockHeight - State.Current.RelativeLockHeight;
                    Warning($"Current state: INVALID_STATE. Please leave/join again after block height {expected}");
                }
                else
                {
                    NameValue($"Current state", State.Current.State);
                }

                var bech32 = new Bech32M(NetworkPrefix);
                NameValue($"Current state from block height", State.SingletonBlockHeight);
                NameValue($"Launcher ID", State.LauncherId);
                NameValue($"Target address (not for plotting)", bech32.PuzzleHashToAddress(State.Current.TargetPuzzleHash));

                var poolPlots = from h in harvesters
                                from plots in h.Plots
                                where plots.PoolContractPuzzleHash == State.P2SingletonPuzzleHash
                                select plots;

                var plotCount = poolPlots.Count();
                NameValue($"Number of plots", plotCount);
                NameValue($"Owner public key", State.Current.OwnerPubkey);

                MarkupLine($"Pool contract address (use ONLY for plotting - do not send money to this address) {bech32.PuzzleHashToAddress(State.P2SingletonPuzzleHash)}");

                if (State.Target is not null)
                {
                    NameValue($"Target state", State.Target.State);
                    NameValue($"Target pooll url", State.Target.PoolUrl);
                }

                if (State.Current.State == PoolSingletonState.SELF_POOLING)
                {
                    var (ConfirmedWalletBalance, UnconfirmedWalletBalance, SpendableBalance, PendingChange, MaxSendAmount, UnspentCoinCount, PendingCoinRemovalCount) = await poolwallet.GetBalance(cts1.Token);
                    NameValue($"Claimable balance", $"[green]{ConfirmedWalletBalance.AsChia("F5")} {NetworkPrefix}[/]");
                }
                else if (State.Current.State == PoolSingletonState.FARMING_TO_POOL)
                {
                    NameValue($"Current pool URL", State.Current.PoolUrl);
                    var poolstate = poolStates.FirstOrDefault(ps => ps.PoolConfig.LauncherId == State.LauncherId);
                    if (poolstate is not null)
                    {
                        NameValue($"Current difficulty", poolstate.CurrentDifficulty);
                        NameValue($"Points balance", poolstate.CurrentPoints);
                        if (poolstate.PointsFound24h.Any())
                        {
                            var pointsAcknowledged = poolstate.PointsAcknowledged24h.Count;
                            var pct = pointsAcknowledged / (double)poolstate.PointsFound24h.Count;
                            NameValue($"Percent Successful Points(24h)", $"{pct:P2}%");
                        }
                        NameValue($"Relative lock height", $"{State.Current.RelativeLockHeight} blocks");
                        try
                        {
                            MarkupLine($"Payout instructions (pool will pay to this address) [green]{bech32.PuzzleHashToAddress(poolstate.PoolConfig.PayoutInstructions)}[/]");
                        }
                        catch
                        {
                            MarkupLine($"Payout instructions (pool will pay you with this) {poolstate.PoolConfig.PayoutInstructions}");
                        }
                    }
                }
                else if (State.Current.State == PoolSingletonState.LEAVING_POOL)
                {
                    var expected = State.SingletonBlockHeight - State.Current.RelativeLockHeight;
                    if (State.Target is not null)
                    {
                        MarkupLine($"Expected to leave after block height: {expected}");
                    }
                }
            });
        }
    }
}
