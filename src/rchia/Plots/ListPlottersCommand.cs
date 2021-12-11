using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots
{
    internal sealed class ListPlottersCommand : EndpointOptions
    {
        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Adding plot directory...", async output =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(output, this);
                var proxy = new PlotterProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var plotters = await proxy.GetPlotters(cts.Token);

                var table = new List<IDictionary<string, object?>>();
                foreach (var plotter in plotters.Values)
                {
                    var row = new Dictionary<string, object?>
                    {
                        { "name", plotter.DisplayName },
                        { "installed", plotter.Installed },
                        { "can_install", plotter.CanInstall },
                        { "version", plotter.Version ?? string.Empty }
                    };

                    table.Add(row);
                }

                output.WriteOutput(table);
            });
        }
    }
}
