using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Farm
{
    internal sealed class ChallengesCommand : EndpointOptions
    {
        [Argument(0, Name = "limit", Default = 20, Description = "Limit the number of challenges shown. Use 0 to disable the limit")]
        public int Limit { get; set; } = 20;

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Daemon);
                var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new FarmTasks(proxy, this);

                await DoWork("Retrieving challenges...", async ctx => { await tasks.Challenges(Limit); });
            });
        }
    }
}
