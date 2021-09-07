﻿using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Plots
{
    internal sealed class ListPlotsCommand : SharedOptions
    {
        [CommandTarget]
        public override async Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Farmer);
                var harvester = new HarvesterProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new PlotsTasks(harvester, this);

                await tasks.List();
            });
        }
    }
}
