﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots
{
    internal sealed class ShowPlotQueueCommand : EndpointOptions
    {
        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Retrieving plot queue...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(ctx, this);
                var proxy = new PlotterProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var q = await proxy.RegisterPlotter(cts.Token);
                var plots = from p in q
                            group p by p.PlotState into g
                            select g;

                foreach (var group in plots)
                {
                    NameValue(group.Key.ToString(), group.Count());
                    if (Verbose)
                    {
                        foreach (var item in group)
                        {
                            WriteLine($"  {item.Id}");
                        }
                    }
                }
            });
        }
    }
}