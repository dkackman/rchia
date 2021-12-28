using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots;

internal sealed class ShowPlotQueueCommand : EndpointOptions
{
    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving plot queue...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(output, this);
            var proxy = new PlotterProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var q = await proxy.RegisterPlotter(cts.Token);
            var plots = from p in q
                        group p by p.PlotState into g
                        select g;

            if (Json)
            {
                var result = from grp in plots
                             select new Dictionary<string, IEnumerable<string>>()
                                {
                                    { grp.Key.ToString(), grp.Select(item => item.Id) }
                                };

                output.WriteOutput(result);
            }
            else
            {
                foreach (var group in plots)
                {
                    output.WriteMarkupLine($"{group.Key} {group.Count()}");
                    if (Verbose)
                    {
                        foreach (var item in group)
                        {
                            output.WriteLine($"  {item.Id}");
                        }
                    }
                }
            }
        });
    }
}
