using System.Threading.Tasks;
using System.ComponentModel;

using chia.dotnet;
using Spectre.Console.Cli;

namespace rchia.Farm
{
    [Description("Summary of farming information")]
    internal sealed class SummaryCommand : AsyncCommand<EndpointSettings>
    {
        public async override Task<int> ExecuteAsync(CommandContext context, EndpointSettings settings)
        {
            using var rpcClient = await ClientFactory2.Factory.CreateWebSocketClient(settings, ServiceNames.Daemon);
            var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
            var tasks = new FarmTasks(proxy, settings);

            await tasks.Summary();

            return 0;
        }
    }
}
