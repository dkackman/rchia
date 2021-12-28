using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Farm;

internal sealed class ChallengesCommand : EndpointOptions
{
    [Argument(0, Name = "limit", Default = 20, Description = "Limit the number of challenges shown. Use 0 to disable the limit")]
    public int Limit { get; init; } = 20;

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving challenges...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Farmer);
            var farmer = new FarmerProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var signagePoints = await farmer.GetSignagePoints(cts.Token);

            var list = signagePoints.Reverse().ToList(); // convert to list to avoid multiple iteration
            var limit = Limit == 0 ? list.Count : Limit;

            var table = from sp in list.Take(limit)
                        select new Dictionary<string, object?>
                        {
                            { "index", sp.SignagePoint.SignagePointIndex },
                            { "hash", new Formattable<string>(sp.SignagePoint.ChallengeHash, hash => hash.Replace("0x", string.Empty)) }
                        };

            output.WriteOutput(table);
            output.WriteMessage($"Showing {table.Count()} of {signagePoints.Count()} challenge{(list.Count == 1 ? string.Empty : "s")}.");
        });
    }
}
