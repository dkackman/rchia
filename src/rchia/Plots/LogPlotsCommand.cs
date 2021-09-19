﻿using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots
{
    internal sealed class LogPlotsCommand : EndpointOptions
    {
        [Option("i", "id", Description = "The id of the plot log. Omit to see logs for all running plots.")]
        public string? Id { get; set; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Plotter);
                var proxy = new PlotterProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new PlotterTasks(proxy, this);

                await DoWork("Retrieivng plot log...", async ctx => { await tasks.Log(Id); });
            });
        }
    }
}
