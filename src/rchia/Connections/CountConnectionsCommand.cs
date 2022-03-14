using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Connections;

internal sealed class CountConnectionsCommand : EndpointOptions
{
    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving connection counts...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Crawler);
            var proxy = new CrawlerProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var counts = await proxy.GetPeerCounts(cts.Token);

            if (Json)
            {
                output.WriteOutput(counts);
            }
            else
            {
                var result = new Dictionary<string, object?>()
                {
                    {"Ipv4Last5Days", counts.Ipv4Last5Days },
                    {"Ipv6Last5Days", counts.Ipv6Last5Days },
                    {"ReliableNodes", counts.ReliableNodes },
                    {"TotalLast5Days", counts.TotalLast5Days },
                };

                foreach (var version in counts.Versions)
                {
                    result.Add(version.Key, version.Value);
                }

                output.WriteOutput(result);
            }
        });
    }
}
