using System;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.PlotNft
{
    internal sealed class GetLoginLinkCommand : SharedOptions
    {
        [Option("l", "launcher-id", Description = "Set the fingerprint to specify which wallet to use")]
        public string LauncherId { get; set; } = string.Empty;

        [CommandTarget]
        public async override Task<int> Run()
        {
            if (string.IsNullOrEmpty(LauncherId))
            {
                throw new InvalidOperationException("A valid launcher id is required. To get the launcher id, use 'plotnft show'.");
            }

            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Wallet);
                var wallet = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new PlotNftTasks(wallet, this);

                await tasks.GetLoginLink(LauncherId);
            });
        }
    }
}
