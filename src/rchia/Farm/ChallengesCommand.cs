using System.Threading.Tasks;
using System.ComponentModel;

using chia.dotnet;
using Spectre.Console.Cli;

namespace rchia.Farm
{
    [Description("Show the latest challenges")]
    internal sealed class ChallengesCommand : AsyncCommand<ChallengesCommand.ChallengesSettings>
    {
        public sealed class ChallengesSettings : EndpointSettings
        {
            [Description("Limit the number of challenges shown. Use 0 to disable the limit")]
            [CommandArgument(0, "[limit]")]
            [DefaultValue(20)]
            public int Limit { get; set; } = 20;
        }

        public async override Task<int> ExecuteAsync(CommandContext context, ChallengesSettings settings)
        {
            using var rpcClient = await ClientFactory2.Factory.CreateWebSocketClient(settings, ServiceNames.Daemon);
            var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
            var tasks = new FarmTasks(proxy, settings);

            await tasks.Challenges(settings.Limit);

            return 0;
        }
    }
}
