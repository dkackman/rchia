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

            var list = signagePoints.Reverse().ToList(); // convert to list to avoid multiple iteration
            var count = Limit == 0 ? list.Count : Limit;

            var table = new List<IDictionary<string, string>>();

            foreach (var sp in list.Take(count))
            {
                var row = new Dictionary<string, string>
                {
                    { "index", sp.SignagePoint.SignagePointIndex.ToString() },
                    { "hash", sp.SignagePoint.ChallengeHash.Replace("0x", string.Empty) }
                };

                table.Add(row);
            }

            output.WriteOutput(table);
            output.Message($"Showing {count} of {list.Count} challenges.");
        });
    }
}
