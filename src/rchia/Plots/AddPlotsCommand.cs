using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots
{
    internal sealed class AddPlotsCommand : EndpointOptions
    {
        [Option("d", "final-dir", Default = ".", Description = "Final directory for plots (relative or absolute)")]
        public string FinalDir { get; set; } = ".";

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, ServiceNames.Harvester);
                var harvester = new HarvesterProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new HarvesterPlotTasks(harvester, this);

                await DoWork("Adding plot directory...", async ctx => { await tasks.Add(FinalDir); });
            });
        }
    }
}
