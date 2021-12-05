using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots;

internal sealed class AddPlotsCommand : EndpointOptions
{
    [Option("d", "final-dir", Default = ".", Description = "Final directory for plots (relative or absolute)")]
    public string FinalDir { get; init; } = ".";

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Adding plot directory...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Harvester);
            var proxy = new HarvesterProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            await proxy.AddPlotDirectory(FinalDir, cts.Token);

            var result = new Dictionary<string, string>()
            {
                { "added", FinalDir }
            };
            output.WriteOutput(result);
        });
    }
}
