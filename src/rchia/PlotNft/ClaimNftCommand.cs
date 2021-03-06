using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.PlotNft;

internal sealed class ClaimNftCommand : WalletCommand
{
    [Option("f", "force", Description = "Do not prompt before claiming rewards")]
    public bool Force { get; init; }

    [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
    public uint Id { get; init; } = 1;

    [Option("m", "fee", Default = 0, Description = "Set the fees for the transaction, in XCH")]
    public decimal Fee { get; init; }

    protected async override Task<bool> Confirm(ICommandOutput output)
    {
        await Task.CompletedTask;

        return output.Confirm($"Are you sure you want to claim rewards for wallet ID {Id}?", Force);
    }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Claiming pool rewards...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var wallet = new PoolWallet(Id, await Login(rpcClient, output));

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            await wallet.Validate(cts.Token);

            var rewards = await wallet.AbsorbRewards(Fee.ToMojo(), cts.Token);

            if (Json)
            {
                output.WriteOutput(rewards);
            }
            else
            {
                PrintTransactionSentTo(output, rewards.Transaction);
            }
        });
    }
}
