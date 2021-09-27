using System;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.PlotNft
{
    internal sealed class GetLoginLinkCommand : EndpointOptions
    {
        [Option("l", "launcher-id", IsRequired = true, Description = "Launcher ID of the plotnft")]
        public string LauncherId { get; init; } = string.Empty;

        [CommandTarget]
        public async override Task<int> Run()
        {
            if (string.IsNullOrEmpty(LauncherId))
            {
                throw new InvalidOperationException("A valid launcher id is required. To get the launcher id, use 'plotnft show'.");
            }

            return await DoWorkAsync("Getting pool login link...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Farmer);
                var farmer = new FarmerProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var link = await farmer.GetPoolLoginLink(LauncherId, cts.Token);

                if (string.IsNullOrEmpty(link))
                {
                    Warning("Was not able to get login link.");
                }
                else
                {
                    MarkupLine($"[link={link}]{link}[/]");
                }
            });
        }
    }
}
