using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Bech32;
using rchia.Commands;

namespace rchia.PlotNft
{
    public enum InitialPoolingState
    {
        local,
        pool
    }

    internal class PlotNftTasks : ConsoleTask<WalletProxy>
    {
        public PlotNftTasks(WalletProxy wallet, IConsoleMessage consoleMessage, int timeoutMilliseconds)
            : base(wallet, consoleMessage, timeoutMilliseconds)
        {
        }

        public async Task Claim(uint walletId)
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var wallet = new PoolWallet(walletId, Service);
            await wallet.Validate(cts.Token);

            var (State, tx) = await wallet.AbsorbRewards(0, cts.Token);

            PrintTransactionSentTo(tx);
        }

        public async Task Inspect(uint walletId)
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var wallet = new PoolWallet(walletId, Service);
            await wallet.Validate(cts.Token);

            var (State, UnconfirmedTransactions) = await wallet.Status(cts.Token);

            ConsoleMessage.WriteLine(State.ToJson());

            foreach (var tx in UnconfirmedTransactions)
            {
                PrintTransactionSentTo(tx);
            }
        }

        private void PrintTransactionSentTo(TransactionRecord tx)
        {
            ConsoleMessage.NameValue("Transaction", tx.Name);
            foreach (var sentTo in tx.SentTo)
            {
                ConsoleMessage.NameValue($"Sent to", sentTo.Peer);
            }

            ConsoleMessage.Helpful($"Do 'rchia wallet get-transaction -tx {tx.Name}' to get status");
        }

        public async Task LeavePool(uint walletId)
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var wallet = new PoolWallet(walletId, Service);
            await wallet.Validate(cts.Token);

            var tx = await wallet.SelfPool(cts.Token);

            PrintTransactionSentTo(tx);
        }

        public async Task<string> ValidatePoolingOptions(InitialPoolingState state, Uri? poolUri)
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);

            if (state == InitialPoolingState.pool && NetworkName == "mainnet" && poolUri is not null && poolUri.Scheme != "https")
            {
                throw new InvalidOperationException($"Pool URLs must be HTTPS on mainnet {poolUri}. Aborting.");
            }

            return $"This operation Will join the wallet with fingerprint [wheat1]{Service.Fingerprint}[/] to [wheat1]{poolUri}[/].\nDo you want to proceed?";
        }

        public async Task Join(uint walletId, Uri poolUri)
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var wallet = new PoolWallet(walletId, Service);
            await wallet.Validate(cts.Token);

            var poolInfo = await GetPoolInfo(poolUri);
            var tx = await wallet.JoinPool(poolInfo.TargetPuzzleHash ?? string.Empty, poolUri.ToString(), poolInfo.RelativeLockHeight, cts.Token);

            PrintTransactionSentTo(tx);
        }

        private async Task<PoolInfo> GetPoolInfo(Uri uri)
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var info = await WalletProxy.GetPoolInfo(uri, cts.Token);

            if (info.RelativeLockHeight > 1000)
            {
                throw new InvalidOperationException("Relative lock height too high for this pool, cannot join");
            }

            if (info.ProtocolVersion != PoolInfo.POOL_PROTOCOL_VERSION)
            {
                throw new InvalidOperationException($"Unsupported version: {info.ProtocolVersion}, should be {PoolInfo.POOL_PROTOCOL_VERSION}");
            }

            return info;
        }

        public async Task Create(InitialPoolingState state, Uri? poolUri)
        {
            var poolInfo = poolUri is not null ? await GetPoolInfo(poolUri) : new PoolInfo();
            var poolState = new PoolState()
            {
                PoolUrl = poolUri?.ToString(),
                State = state == InitialPoolingState.pool ? PoolSingletonState.FARMING_TO_POOL : PoolSingletonState.SELF_POOLING,
                TargetPuzzleHash = poolInfo.TargetPuzzleHash!,
                RelativeLockHeight = poolInfo.RelativeLockHeight
            };

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var (tx, launcherId, p2SingletonHash) = await Service.CreatePoolWallet(poolState, null, null, cts.Token);
            ConsoleMessage.NameValue("Launcher Id", launcherId);
            PrintTransactionSentTo(tx);
        }

        // https://testpool.xchpool.org
        public async Task GetLoginLink(string launcherId)
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            if (Service.RpcClient is not WebSocketRpcClient)
            {
                throw new InvalidOperationException("This command requires a daemon endpoint");
            }

            var farmer = new FarmerProxy(Service.RpcClient, Service.OriginService);
            var link = await farmer.GetPoolLoginLink(launcherId, cts.Token);

            if (string.IsNullOrEmpty(link))
            {
                ConsoleMessage.Warning("Was not able to get login link.");
            }
            else
            {
                ConsoleMessage.MarkupLine($"[link={link}]{link}[/]");
            }
        }

        public async Task Show(uint? walletId)
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            if (Service.RpcClient is not WebSocketRpcClient)
            {
                throw new InvalidOperationException("This command requires a daemon endpoint");
            }

            var allWallets = await Service.GetWallets(cts.Token);
            var poolingWallets = allWallets.Where(w => w.Type == WalletType.POOLING_WALLET);
            if (!poolingWallets.Any())
            {
                throw new InvalidOperationException("There are no pooling wallets on this node.");
            }

            if (walletId.HasValue)
            {
                // since the command specified a specific wallet narrow the result to just that
                poolingWallets = poolingWallets.Where(w => w.Id == walletId.Value);
                if (!poolingWallets.Any())
                {
                    throw new InvalidOperationException($"Wallet with id: {walletId} is not a pooling wallet or doesn't exist. Please provide a different id.");
                }
            }

            var farmer = new FarmerProxy(Service.RpcClient, Service.OriginService);
            var poolStates = await farmer.GetPoolState(cts.Token);
            var harvesters = await farmer.GetHarvesters(cts.Token);
            var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);
            var height = await Service.GetHeightInfo(cts.Token);
            var (GenesisInitialized, Synced, Syncing) = await Service.GetSyncStatus(cts.Token);

            ConsoleMessage.NameValue($"Wallet height", height);
            ConsoleMessage.NameValue($"Sync status", $"{(Synced ? "" : "not ")}synced");

            foreach (var w in poolingWallets)
            {
                ConsoleMessage.NameValue($"Wallet", w.Id);
                var poolwallet = new PoolWallet(w.Id, Service);
                using var cts1 = new CancellationTokenSource(TimeoutMilliseconds);
                var (State, UnconfirmedTransactions) = await poolwallet.Status(cts1.Token);

                if (State.Current.State == PoolSingletonState.LEAVING_POOL)
                {
                    var expected = State.SingletonBlockHeight - State.Current.RelativeLockHeight;
                    ConsoleMessage.Warning($"Current state: INVALID_STATE. Please leave/join again after block height {expected}");
                }
                else
                {
                    ConsoleMessage.NameValue($"Current state", State.Current.State);
                }

                var bech32 = new Bech32M(NetworkPrefix);
                ConsoleMessage.NameValue($"Current state from block height", State.SingletonBlockHeight);
                ConsoleMessage.NameValue($"Launcher ID", State.LauncherId);
                ConsoleMessage.NameValue($"Target address (not for plotting)", bech32.PuzzleHashToAddress(State.Current.TargetPuzzleHash));

                var poolPlots = from h in harvesters
                                from plots in h.Plots
                                where plots.PoolContractPuzzleHash == State.P2SingletonPuzzleHash
                                select plots;

                var plotCount = poolPlots.Count();
                ConsoleMessage.NameValue($"Number of plots", plotCount);
                ConsoleMessage.NameValue($"Owner public key", State.Current.OwnerPubkey);

                ConsoleMessage.MarkupLine($"Pool contract address (use ONLY for plotting - do not send money to this address) {bech32.PuzzleHashToAddress(State.P2SingletonPuzzleHash)}");

                if (State.Target is not null)
                {
                    ConsoleMessage.NameValue($"Target state", State.Target.State);
                    ConsoleMessage.NameValue($"Target pooll url", State.Target.PoolUrl);
                }

                if (State.Current.State == PoolSingletonState.SELF_POOLING)
                {
                    var (ConfirmedWalletBalance, UnconfirmedWalletBalance, SpendableBalance, PendingChange, MaxSendAmount, UnspentCoinCount, PendingCoinRemovalCount) = await poolwallet.GetBalance(cts1.Token);
                    ConsoleMessage.NameValue($"Claimable balance", $"[green]{ConfirmedWalletBalance.AsChia("F5")} {NetworkPrefix}[/]");
                }
                else if (State.Current.State == PoolSingletonState.FARMING_TO_POOL)
                {
                    ConsoleMessage.NameValue($"Current pool URL", State.Current.PoolUrl);
                    var poolstate = poolStates.FirstOrDefault(ps => ps.PoolConfig.LauncherId == State.LauncherId);
                    if (poolstate is not null)
                    {
                        ConsoleMessage.NameValue($"Current difficulty", poolstate.CurrentDifficulty);
                        ConsoleMessage.NameValue($"Points balance", poolstate.CurrentPoints);
                        if (poolstate.PointsFound24h.Any())
                        {
                            var pointsAcknowledged = poolstate.PointsAcknowledged24h.Count;
                            var pct = pointsAcknowledged / (double)poolstate.PointsFound24h.Count;
                            ConsoleMessage.NameValue($"Percent Successful Points(24h)", $"{pct:P2}%");
                        }
                        ConsoleMessage.NameValue($"Relative lock height", $"{State.Current.RelativeLockHeight} blocks");
                        try
                        {
                            ConsoleMessage.MarkupLine($"Payout instructions (pool will pay to this address) [green]{bech32.PuzzleHashToAddress(poolstate.PoolConfig.PayoutInstructions)}[/]");
                        }
                        catch
                        {
                            ConsoleMessage.MarkupLine($"Payout instructions (pool will pay you with this) {poolstate.PoolConfig.PayoutInstructions}");
                        }
                    }
                }
                else if (State.Current.State == PoolSingletonState.LEAVING_POOL)
                {
                    var expected = State.SingletonBlockHeight - State.Current.RelativeLockHeight;
                    if (State.Target is not null)
                    {
                        ConsoleMessage.MarkupLine($"Expected to leave after block height: {expected}");
                    }
                }
            }
        }
    }
}
