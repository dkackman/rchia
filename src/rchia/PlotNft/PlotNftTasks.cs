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
        private const byte POOL_PROTOCOL_VERSION = 1;

        public PlotNftTasks(WalletProxy wallet, IConsoleMessage consoleMessage)
            : base(wallet, consoleMessage)
        {
        }

        public async Task Claim(uint walletId)
        {
            using var cts = new CancellationTokenSource(30000);
            var wallet = new PoolWallet(walletId, Service);
            await wallet.Validate(cts.Token);

            var (State, tx) = await wallet.AbsorbRewards(0, cts.Token);
            Console.WriteLine($"Absorb rewards submitted to node: {tx.SentTo.FirstOrDefault()}");
            Console.WriteLine($"Do 'rchia wallet get-transaction -tx {tx.Name}' to get status");
        }

        public async Task Inspect(uint walletId)
        {
            using var cts = new CancellationTokenSource(30000);
            var wallet = new PoolWallet(walletId, Service);
            await wallet.Validate(cts.Token);

            var (State, UnconfirmedTransactions) = await wallet.Status(cts.Token);

            Console.WriteLine(State);

            foreach (var tx in UnconfirmedTransactions)
            {
                Console.WriteLine($"Transaction Id: {tx.Name}");
                foreach (var sentTo in tx.SentTo)
                {
                    Console.WriteLine($"Sent to {sentTo.Peer}");
                }
            }
        }

        public async Task LeavePool(uint walletId)
        {
            using var cts = new CancellationTokenSource(30000);
            var wallet = new PoolWallet(walletId, Service);
            await wallet.Validate(cts.Token);

            var tx = await wallet.SelfPool(cts.Token);

            Console.WriteLine($"Self pooling transaction submitted to node: {tx.SentTo.FirstOrDefault()}");
            Console.WriteLine($"Do 'rchia wallet get-transaction -tx {tx.Name}' to get status");
        }

        public async Task<string> ValidatePoolingOptions(InitialPoolingState state, Uri? poolUri)
        {
            using var cts = new CancellationTokenSource(30000);

            var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);

            if (state == InitialPoolingState.pool && NetworkName == "mainnet" && poolUri is not null && poolUri.Scheme != "https")
            {
                throw new InvalidOperationException($"Pool URLs must be HTTPS on mainnet {poolUri}. Aborting.");
            }

            return $"This operation Will join pool {poolUri} with Plot NFT {Service.Fingerprint}.";
        }

        public async Task Join(uint walletId, Uri poolUri)
        {
            using var cts = new CancellationTokenSource(30000);
            var wallet = new PoolWallet(walletId, Service);
            await wallet.Validate(cts.Token);

            var poolInfo = await GetPoolInfo(poolUri);
            var tx = await wallet.JoinPool(poolInfo.TargetPuzzleHash ?? string.Empty, poolUri.ToString(), poolInfo.RelativeLockHeight, cts.Token);

            Console.WriteLine($"Join pool transaction submitted to node: {tx.SentTo.FirstOrDefault()}");
            Console.WriteLine($"Do 'rchia wallet get-transaction -tx {tx.Name}' to get status");
        }

        private async static Task<PoolInfo> GetPoolInfo(Uri uri)
        {
            using var cts = new CancellationTokenSource(30000);
            var info = await WalletProxy.GetPoolInfo(uri, cts.Token);

            if (info.RelativeLockHeight > 1000)
            {
                throw new InvalidOperationException("Relative lock height too high for this pool, cannot join");
            }

            if (info.ProtocolVersion != POOL_PROTOCOL_VERSION)
            {
                throw new InvalidOperationException($"Unsupported version: {info.ProtocolVersion}, should be { POOL_PROTOCOL_VERSION}");
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

            using var cts = new CancellationTokenSource(30000);
            var (transaction, launcherId, p2SingletonHash) = await Service.CreatePoolWallet(poolState, null, null, cts.Token);
            Console.WriteLine($"Launcher Id: {launcherId}");
            Console.WriteLine($"Do 'rchia wallet get-transaction -tx {transaction.Name}' to get status");
        }

        // https://testpool.xchpool.org
        public async Task GetLoginLink(string launcherId)
        {
            using var cts = new CancellationTokenSource(30000);

            var farmer = new FarmerProxy(Service.RpcClient, Service.OriginService);
            var link = await farmer.GetPoolLoginLink(launcherId, cts.Token);

            if (string.IsNullOrEmpty(link))
            {
                Console.WriteLine("Was not able to get login link.");
            }
            else
            {
                Console.WriteLine(link);
            }
        }

        public async Task Show(uint? walletId)
        {
            using var cts = new CancellationTokenSource(30000);

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

            Console.WriteLine($"Wallet height: {height}");
            Console.WriteLine($"Sync status:{(Synced ? "" : " not")} synced");

            foreach (var w in poolingWallets)
            {
                Console.WriteLine($"Wallet {w.Id}");
                var poolwallet = new PoolWallet(w.Id, Service);
                using var cts1 = new CancellationTokenSource(30000);
                var (State, UnconfirmedTransactions) = await poolwallet.Status(cts1.Token);

                if (State.Current.State == PoolSingletonState.LEAVING_POOL)
                {
                    var expected = State.SingletonBlockHeight - State.Current.RelativeLockHeight;
                    Console.WriteLine($"Current state: INVALID_STATE. Please leave/join again after block height {expected}");
                }
                else
                {
                    Console.WriteLine($"Current state: {State.Current.State}");
                }

                var bech32 = new Bech32M(NetworkPrefix);
                Console.WriteLine($"Current state from block height: {State.SingletonBlockHeight}");
                Console.WriteLine($"Launcher ID: {State.LauncherId}");
                Console.WriteLine($"Target address (not for plotting): {bech32.PuzzleHashToAddress(State.Current.TargetPuzzleHash)}");

                var poolPlots = from h in harvesters
                                from plots in h.Plots
                                where plots.PoolContractPuzzleHash == State.P2SingletonPuzzleHash
                                select plots;

                var plotCount = poolPlots.Count();
                Console.WriteLine($"Number of plots: {plotCount}");
                Console.WriteLine($"Owner public key: {State.Current.OwnerPubkey}");

                Console.WriteLine($"Pool contract address (use ONLY for plotting - do not send money to this address): {bech32.PuzzleHashToAddress(State.P2SingletonPuzzleHash)}");

                if (State.Target is not null)
                {
                    Console.WriteLine($"Target state: {State.Target.State}");
                    Console.WriteLine($"Target pooll url: {State.Target.PoolUrl}");
                }

                if (State.Current.State == PoolSingletonState.SELF_POOLING)
                {
                    var (ConfirmedWalletBalance, UnconfirmedWalletBalance, SpendableBalance, PendingChange, MaxSendAmount, UnspentCoinCount, PendingCoinRemovalCount) = await poolwallet.GetBalance(cts1.Token);
                    Console.WriteLine($"Claimable balance: {ConfirmedWalletBalance.AsChia("F5")} {NetworkPrefix}");
                }
                else if (State.Current.State == PoolSingletonState.FARMING_TO_POOL)
                {
                    Console.WriteLine($"Current pool URL: {State.Current.PoolUrl}");
                    var poolstate = poolStates.FirstOrDefault(ps => ps.PoolConfig.LauncherId == State.LauncherId);
                    if (poolstate is not null)
                    {
                        Console.WriteLine($"Current difficulty: {poolstate.CurrentDifficulty}");
                        Console.WriteLine($"Points balance: {poolstate.CurrentPoints}");
                        if (poolstate.PointsFound24h.Any())
                        {
                            var pointsAcknowledged = poolstate.PointsAcknowledged24h.Count;
                            var pct = pointsAcknowledged / (double)poolstate.PointsFound24h.Count;
                            Console.WriteLine($"Percent Successful Points(24h): {pct:P2}%");
                        }

                        try
                        {
                            Console.WriteLine($"Payout instructions (pool will pay to this address): {bech32.PuzzleHashToAddress(poolstate.PoolConfig.PayoutInstructions)}");
                        }
                        catch
                        {
                            Console.WriteLine($"Payout instructions (pool will pay you with this): {poolstate.PoolConfig.PayoutInstructions}");
                        }
                    }
                    Console.WriteLine($"Relative lock height: {State.Current.RelativeLockHeight} blocks");

                }
                else if (State.Current.State == PoolSingletonState.LEAVING_POOL)
                {
                    var expected = State.SingletonBlockHeight - State.Current.RelativeLockHeight;
                    if (State.Target is not null)
                    {
                        Console.WriteLine($"Expected to leave after block height: {expected}");
                    }
                }
            }
        }
    }
}
