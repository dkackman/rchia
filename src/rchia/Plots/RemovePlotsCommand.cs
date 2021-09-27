using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots
{
    internal sealed class RemovePlotsCommand : EndpointOptions
    {
        [Option("d", "final-dir", Default = ".", Description = "Final directory for plots (relative or absolute)")]
        public string FinalDir { get; init; } = ".";

        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Removing plot directory...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Harvester);
                var proxy = new HarvesterProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                await proxy.RemovePlotDirectory(FinalDir, cts.Token);

                MarkupLine($"Removed [wheat1]{FinalDir}[/]");
            });
        }
    }
}
