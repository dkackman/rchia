using System.Linq;
using System.Collections.Generic;
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
            output.WriteOutput(signagePoints);

            var list = signagePoints.Reverse().ToList(); // convert to list to avoid multiple iteration
            var limit = Limit == 0 ? list.Count : Limit;

            var table = new List<IDictionary<string, object?>>();

            foreach (var sp in list.Take(limit))
            {
                var row = new Dictionary<string, object?>
                {
                    { "hash", sp.SignagePoint.ChallengeHash.Replace("0x", string.Empty) },
                    { "index", sp.SignagePoint.SignagePointIndex }
                };

                table.Add(row);
            }

            output.WriteOutput(table);
            output.Message($"Showing {table.Count()} of {signagePoints.Count()} challenge{(list.Count == 1 ? string.Empty : "s")}.");
        });
    }
}
