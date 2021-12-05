using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using chia.dotnet;
using rchia.Commands;

namespace rchia.PlotNft;

internal sealed class GetLoginLinkCommand : EndpointOptions
{
    [Option("l", "launcher-id", IsRequired = true, Description = "Launcher ID of the plotnft")]
    public string LauncherId { get; init; } = string.Empty;

    [CommandTarget]
    public async Task<int> Run()
    {
        if (string.IsNullOrEmpty(LauncherId))
        {
            throw new InvalidOperationException("A valid launcher id is required. To get the launcher id, use 'plotnft show'.");
        }

        return await DoWorkAsync("Getting pool login link...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Farmer);
            var farmer = new FarmerProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var link = await farmer.GetPoolLoginLink(LauncherId, cts.Token);

            var result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(link))
            {
                output.Warning("Was not able to get login link.");
            }
            else
            {
                result.Add("link", link);
            }
            output.WriteOutput(result);
        });
    }
}
