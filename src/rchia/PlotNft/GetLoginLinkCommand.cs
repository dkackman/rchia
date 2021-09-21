using System;
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

            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Wallet, TimeoutMilliseconds);
                var wallet = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new PlotNftTasks(wallet, this);

                await DoWork("Getting pool login link...", async ctx => await tasks.GetLoginLink(LauncherId));
            });
        }
    }
}
