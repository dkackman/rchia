using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots
{
    internal sealed class AddPlotsCommand : EndpointOptions
    {
        [Option("d", "final-dir", Default = ".", Description = "Final directory for plots (relative or absolute)")]
        public string FinalDir { get; init; } = ".";

        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Adding plot directory...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Harvester);
                var proxy = new HarvesterProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                await proxy.AddPlotDirectory(FinalDir, cts.Token);

                MarkupLine($"Added [wheat1]{FinalDir}[/]");
            });
        }
    }
}
