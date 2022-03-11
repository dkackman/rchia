using System.Threading;
using System.Threading.Tasks;
using rchia.Commands;
using chia.dotnet;

namespace rchia.Cats;

internal sealed class GetOfferCommand : WalletCommand
{
    [Option("i", "id", IsRequired = true, Description = "The ID of the offer that you wish to examine")]
    public string Id { get; init; } = string.Empty;

    [Option("fc", "file-contents", Description = "Include the offer contents")]
    public bool FileContents { get; init; }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving offer...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var proxy = await Login(rpcClient, output);
            var tradeManager = new TradeManager(proxy);
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            var offer = await tradeManager.GetOffer(Id, FileContents, cts.Token);

            output.WriteOutput(offer);
        });
    }
}
