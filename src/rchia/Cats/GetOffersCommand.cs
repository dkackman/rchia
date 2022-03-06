using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using rchia.Commands;
using chia.dotnet;

namespace rchia.Cats;

internal sealed class GetOffersCommand : WalletCommand
{
    [Option("em", "exclude-my-offers", Description = "Exclude your own offers from the output")]
    public bool ExcludeMyOffers { get; init; }

    [Option("et", "exclude-taken-offers", Description = "Exclude offers that you've accepted from the output")]
    public bool ExcludeTakenOffers { get; init; }

    [Option("ic", "include-completed", Description = "Include offers that have been confirmed/cancelled or failed")]
    public bool IncludeCompleted { get; init; }

    [Option("s", "summaries", Description = "Show the assets being offered and requested for each offer")]
    public bool Summaries { get; init; }

    [Option("r", "reverse", Description = "Reverse the order of the output")]
    public bool Reverse { get; init; }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving offers...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var proxy = await Login(rpcClient, output);
            var tradeManager = new TradeManager(proxy);
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            var offers = await tradeManager.GetOffers(excludeMyOffers: ExcludeMyOffers,
                                                       excludeTakenOffers: ExcludeTakenOffers,
                                                       includeCompleted: IncludeCompleted,
                                                       reverse: Reverse,
                                                       fileContents: Summaries,
                                                       cancellationToken: cts.Token);

            output.WriteOutput(offers);
        });
    }
}
