using System;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Show
{
    internal sealed class PruneCommand : EndpointOptions
    {
        [Option("a", "age", Default = 12, Description = "Prune nodes that haven't sent data in this number of hours")]
        public int Age { get; set; } = 12;

        [CommandTarget]
        public async override Task<int> Run()
        {
            if (Age < 1)
            {
                throw new InvalidOperationException("Age must be 1 or more");
            }

            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, ServiceNames.FullNode);
                var fullNode = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new ShowTasks(fullNode, this);

                await tasks.Prune(Age);
            });
        }
    }
}
