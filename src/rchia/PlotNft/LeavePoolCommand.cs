using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.PlotNft;

internal sealed class LeavePoolCommand : WalletCommand
{
    [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
    public uint Id { get; init; } = 1;

    [Option("f", "force", Description = "Do not prompt before nft creation")]
    public bool Force { get; init; }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Leaving pool...", async output =>
        {
            if (output.Confirm($"Are you sure you want to start self-farming with Plot NFT on wallet id {Id}?", Force))
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
                var wallet = new PoolWallet(Id, await Login(rpcClient, output));

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                await wallet.Validate(cts.Token);

                var tx = await wallet.SelfPool(cts.Token);

                if (Json)
                {
                    output.WriteOutput(tx);
                }
                else
                {
                    PrintTransactionSentTo(output, tx);
                }
            }
        });
    }
}
