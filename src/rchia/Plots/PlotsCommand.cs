using System;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Plots
{
    [Command("plots", Description = "Manage your plots.\nRequires a daemon endpoint.")]
    internal sealed class PlotsCommand : SharedOptions
    {
        [Option("s", "show", Description = "Shows the directory of current plots")]
        public bool Show { get; set; }

        public override async Task<int> Run()
        {
            try
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Farmer);
                var harvester = new HarvesterProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new PlotsTasks(harvester, this);

                if (Show)
                {
                    await tasks.Show();
                }
                else
                {
                    throw new InvalidOperationException("Unrecognized command");
                }

                return 0;
            }
            catch (Exception e)
            {
                Message(e);

                return -1;
            }
        }
    }
}
