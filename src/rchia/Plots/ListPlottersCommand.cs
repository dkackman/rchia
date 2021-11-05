using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using Spectre.Console;

namespace rchia.Plots
{
    internal sealed class ListPlottersCommand : EndpointOptions
    {
        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Adding plot directory...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(ctx, this);
                var proxy = new PlotterProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var plotters = await proxy.GetPlotters(cts.Token);

                var table = new Table
                {
                    Title = new TableTitle($"[orange3]Plotters[/]")
                };

                table.AddColumn("[orange3]Name[/]");
                table.AddColumn("[orange3]Installed[/]");
                table.AddColumn("[orange3]Can Install[/]");
                table.AddColumn("[orange3]Version[/]");

                foreach (var plotter in plotters.Values)
                {
                    table.AddRow(plotter.DisplayName, plotter.Installed.ToString(), plotter.CanInstall.ToString(), (plotter.Version is not null ? plotter.Version : string.Empty));
                }

                AnsiConsole.Write(table);
            });
        }
    }
}
