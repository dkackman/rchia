using System.Threading;
using System.Threading.Tasks;
using rchia.Commands;
using chia.dotnet;

namespace rchia.Cats;

internal sealed class TakeOfferCommand : WalletCommand
{
    [Argument(0, Name = "OFFER", Description = "Path to an offer file or bech32 encoded hex string")]
    public string Offer { get; init; } = string.Empty;

    [Option("m", "fee", Default = 0, Description = "Set the fees for the transaction, in XCH")]
    public decimal Fee { get; init; }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Taking offer...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var proxy = await Login(rpcClient, output);
            var tradeManager = new TradeManager(proxy);
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            var trade = await tradeManager.TakeOffer(Offer, Fee.ToMojo(), cts.Token);

            output.WriteOutput(trade);
        });
    }
}
