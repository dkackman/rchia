using System.Threading;
using System.Threading.Tasks;
using rchia.Commands;
using chia.dotnet;

namespace rchia.Cats;

internal sealed class CheckOfferCommand : WalletCommand
{
    [Argument(0, Name = "OFFER", Description = "Path to an offer file or bech32 encoded hex string")]
    public string Offer { get; init; } = string.Empty;

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Checking offer...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var proxy = await Login(rpcClient, output);
            var tradeManager = new TradeManager(proxy);
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            var valid = await tradeManager.CheckOfferValidity(Offer,  cts.Token);

            output.WriteOutput("offer_is_valid", valid, Verbose);
        });
    }
}
