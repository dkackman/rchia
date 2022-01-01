using System;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.PlotNft;

public enum InitialPoolingState
{
    pool,
    local
}

internal sealed class CreatePlotNftCommand : WalletCommand
{
    [Option("u", "pool-url", Description = "HTTPS host:port of the pool to join. Omit for self pooling")]
    public Uri? PoolUrl { get; init; }

    [Option("s", "state", IsRequired = true, Description = "Initial state of Plot NFT")]
    public InitialPoolingState State { get; init; }

    [Option("f", "force", Description = "Do not prompt before nft creation")]
    public bool Force { get; init; }

    protected async override Task<bool> Confirm(ICommandOutput output)
    {
        using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
        var proxy = await Login(rpcClient, output);

        var msg = await proxy.ValidatePoolingOptions(State == InitialPoolingState.pool, PoolUrl, TimeoutMilliseconds);

        return output.Confirm(msg, Force);
    }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Creating pool NFT and wallet...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var proxy = await Login(rpcClient, output);

            var poolInfo = PoolUrl is not null ? await PoolUrl.GetPoolInfo(TimeoutMilliseconds) : new PoolInfo();
            var poolState = new PoolState()
            {
                PoolUrl = PoolUrl?.ToString(),
                State = State == InitialPoolingState.pool ? PoolSingletonState.FARMING_TO_POOL : PoolSingletonState.SELF_POOLING,
                TargetPuzzleHash = poolInfo.TargetPuzzleHash!,
                RelativeLockHeight = poolInfo.RelativeLockHeight
            };

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var result = await proxy.CreatePoolWallet(poolState, null, null, cts.Token);

            if (Json)
            {
                output.WriteOutput(result);
            }
            else
            {
                output.WriteOutput("launcher_id", result.launcherId, Verbose);

                PrintTransactionSentTo(output, result.transaction);
            }
        });
    }
}
