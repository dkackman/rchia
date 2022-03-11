using System.Threading;
using System.Threading.Tasks;
using rchia.Commands;
using chia.dotnet;

namespace rchia.Cats;

internal sealed class CancelOffer : WalletCommand
{
    [Option("i", "id", IsRequired = true, Description = "The ID of the offer that you wish to examine")]
    public string Id { get; init; } = string.Empty;

    [Option("s", "secure", Description = "Cancel using a transaction")]
    public bool Secure { get; init; }

    [Option("m", "fee", Default = 0, Description = "Set the fees for the cancellation transaction, in XCH")]
    public decimal Fee { get; init; }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Cancelling offer...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var proxy = await Login(rpcClient, output);
            var tradeManager = new TradeManager(proxy);
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            if (Secure)
            {
                await tradeManager.CancelOffer(Id, Fee.ToMojo(), cts.Token);
            }
            else
            {
                await tradeManager.CancelOffer(Id, cts.Token);
            }

            output.WriteOutput("status", "success", Verbose);
        });
    }
}
