using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Show
{
    internal sealed class PruneCommand : EndpointOptions
    {
        [Option("a", "age", Default = 12, Description = "Prune nodes that haven't sent data in this number of hours")]
        public int Age { get; init; } = 12;

        [CommandTarget]
        public async override Task<int> Run()
        {
            if (Age < 1)
            {
                throw new InvalidOperationException("Age must be 1 or more");
            }

            return await DoWork2("Pruning connections...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.FullNode);
                var proxy = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var cutoff = DateTime.UtcNow - new TimeSpan(Age, 0, 0);
                MarkupLine($"Pruning connections that haven't sent a message since [wheat1]{cutoff.ToLocalTime()}[/]");

                var connections = await proxy.GetConnections(cts.Token);
                var n = 0;
                // only prune other full nodes, not famers, harvesters, and wallets etc
                foreach (var connection in connections.Where(c => c.Type == NodeType.FULL_NODE && c.LastMessageDateTime < cutoff))
                {
                    using var cts1 = new CancellationTokenSource(TimeoutMilliseconds);
                    await proxy.CloseConnection(connection.NodeId, cts1.Token);
                    MarkupLine($"Closed connection at [wheat1]{connection.PeerHost}:{connection.PeerServerPort}[/] that last updated [wheat1]{connection.LastMessageDateTime.ToLocalTime()}[/]");
                    n++;
                }

                MarkupLine($"Pruned [wheat1]{n}[/] connection{(n == 1 ? string.Empty : "s")}");
            });
        }
    }
}
