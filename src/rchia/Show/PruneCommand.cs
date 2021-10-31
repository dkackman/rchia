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
        [Option("a", "age", Description = "Prune nodes that haven't sent data in this number of hours")]
        public int Age { get; init; }

        [Option("b", "behind", Description = "Prune nodes that are this number behind sync tip height")]
        public ulong Behind { get; init; }

        [CommandTarget]
        public async Task<int> Run()
        {
            if (Age < 1 && Behind < 1)
            {
                throw new InvalidOperationException("Either --age or --behind options must be 1 or more");
            }

            return await DoWorkAsync("Pruning connections...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.FullNode);
                var fullNode = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);

                var n = 0;


                if (Age > 0)
                {
                    var cutoff = DateTime.UtcNow - new TimeSpan(Age, 0, 0);
                    MarkupLine($"Pruning connections that haven't sent a message since [wheat1]{cutoff.ToLocalTime()}[/]");

                    using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                    var connections = await fullNode.GetConnections(cts.Token);
                    // only prune other full nodes, not famers, harvesters, and wallets etc
                    foreach (var connection in connections.Where(c => c.Type == NodeType.FULL_NODE && c.LastMessageDateTime < cutoff))
                    {
                        using var cts1 = new CancellationTokenSource(TimeoutMilliseconds);
                        await fullNode.CloseConnection(connection.NodeId, cts1.Token);
                        MarkupLine($"Closed connection at [wheat1]{connection.PeerHost}:{connection.PeerServerPort}[/] that last updated [wheat1]{connection.LastMessageDateTime.ToLocalTime()}[/]");
                        n++;
                    }
                }
                else // (Behind > 0)
                {
                    using var cts = new CancellationTokenSource(TimeoutMilliseconds);

                    var state = await fullNode.GetBlockchainState(cts.Token);
                    MarkupLine($"Pruning connections that with a peak less than [wheat1]{state.Sync.SyncProgressHeight - Behind}[/]");
                    var maxHeight = state.Sync.SyncProgressHeight - Behind;
                    var connections = await fullNode.GetConnections(cts.Token);
                    // only prune other full nodes, not famers, harvesters, and wallets etc
                    foreach (var connection in connections.Where(c => c.Type == NodeType.FULL_NODE && c.PeakHeight < maxHeight))
                    {
                        using var cts1 = new CancellationTokenSource(TimeoutMilliseconds);

                        await fullNode.CloseConnection(connection.NodeId, cts1.Token);
                        MarkupLine($"Closed connection at [wheat1]{connection.PeerHost}:{connection.PeerServerPort}[/] with a peak of [wheat1]{connection.PeakHeight}[/]");
                        n++;
                    }

                    MarkupLine($"Pruned [wheat1]{n}[/] connection{(n == 1 ? string.Empty : "s")}");
                }
            });
        }
    }
}
