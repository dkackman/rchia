using System.Threading;
using System.Threading.Tasks;
using rchia.Commands;
using chia.dotnet;

namespace rchia.Cats;

internal sealed class CancelOffer : WalletCommand
{
    [Option("i", "id", IsRequired = true, Description = "The trade ID of the offer that you wish to examine")]
    public string Id { get; init; } = string.Empty;

    [Option("s", "secure", Description = "Cancel using a transaction")]
    public bool Secure { get; init; }

    [Option("m", "fee", Default = 0, Description = "Set the fees for the cancellation transaction, in XCH")]
    public decimal Fee { get; init; }

    [Option("f", "force", Description = "Cancel the offer without prompting for confirmation")]
    public bool Force { get; init; }

    protected async override Task<bool> Confirm(ICommandOutput output)
    {
        await Task.CompletedTask;

        return output.Confirm($"Are you sure you want to cancel offer {Id}?", Force);
    }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Cancelling offer...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var proxy = await Login(rpcClient, output);
            var tradeManager = new TradeManager(proxy);
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            await tradeManager.CancelOffer(Id, Secure, Fee.ToMojo(), cts.Token);

            output.WriteOutput("status", "success", Verbose);
        });
    }
}
