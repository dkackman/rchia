using System.Threading;
using System.Threading.Tasks;
using rchia.Commands;
using chia.dotnet;

namespace rchia.Cats;

internal sealed class MakeOfferCommand : WalletCommand
{
    [Option("o", "offer", IsRequired = true, Description = "A wallet id to offer and the amount to offer (formatted like wallet_id: amount)")]
    public string Offer { get; init; } = string.Empty;

    [Option("r", "request", IsRequired = true, Description = "A wallet id of an asset to receive and the amount you wish to receive(formatted like wallet_id: amount)")]
    public string Request { get; init; } = string.Empty;

    [Option("m", "fee", Default = 0, Description = "Set the fees for the cancellation transaction, in XCH")]
    public decimal Fee { get; init; }

    [Option("e", "examine-only", Description = "Print the summary of the offer file but do not make it.")]
    public bool ExamineOnly { get; init; }

    [Option("fp", "file-path", IsRequired = true, Description = "The path to write the generated offer file to.")]
    public string FilePath { get; init; } = string.Empty;

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Making offer...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var proxy = await Login(rpcClient, output);
            var tradeManager = new TradeManager(proxy);
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            var offer = await tradeManager.CreateOfferForIds(null, Fee.ToMojo(), ExamineOnly, cts.Token);

            output.WriteOutput(offer);
        });
    }
}
