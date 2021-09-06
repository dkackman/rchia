using System;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Plots
{
    internal sealed class CreatePlotsCommand : SharedOptions
    {
        [Option("d", "final-dir", Default = ".", Description = "Final directory for plots (relative or absolute)")]
        public string FinalDir { get; set; } = ".";

        [CommandTarget]
        public override async Task<int> Run()
        {
            try
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Farmer);
                var harvester = new HarvesterProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new PlotsTasks(harvester, this);

                await tasks.Add(FinalDir);

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
