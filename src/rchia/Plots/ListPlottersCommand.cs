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

                var table = new List<IDictionary<string, string>>();
                foreach (var plotter in plotters.Values)
                {
                    var row = new Dictionary<string, string>
                    {
                        { "name", plotter.DisplayName },
                        { "installed", plotter.Installed.ToString() },
                        { "can_install", plotter.CanInstall.ToString() },
                        { "version", plotter.Version is not null ? plotter.Version : string.Empty }
                    };

                    table.Add(row);
                }

                output.WriteOutput(table);
            });
        }
    }
}
