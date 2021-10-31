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
        [Argument(0, Name = "blocks", Default = 1, Description = "Prune nodes that are this many blocks behind the sync tip height")]
        public ulong Blocks { get; init; } = 1;

        [CommandTarget]
        public async Task<int> Run()
        {
            if (Blocks < 1)
            {
                throw new InvalidOperationException("A number of blocks must be provided");
            }

            return await DoWorkAsync("Pruning connections...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.FullNode);
                var fullNode = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var state = await fullNode.GetBlockchainState(cts.Token);
                if (Blocks > state.Sync.SyncProgressHeight)
                {
                    throw new InvalidOperationException("Offset is greater than synced height. Aborting.");
                }

                MarkupLine($"Pruning connections that with a peak less than [wheat1]{state.Sync.SyncProgressHeight - Blocks}[/]");
                var maxHeight = state.Sync.SyncProgressHeight - Blocks;

                var connections = await fullNode.GetConnections(cts.Token);
                // only prune other full nodes, not famers, harvesters, and wallets etc
                var n = 0;
                foreach (var connection in connections.Where(c => c.Type == NodeType.FULL_NODE && c.PeakHeight < maxHeight))
                {
                    using var cts1 = new CancellationTokenSource(TimeoutMilliseconds);

                    await fullNode.CloseConnection(connection.NodeId, cts1.Token);
                    MarkupLine($"Closed connection at [wheat1]{connection.PeerHost}:{connection.PeerServerPort}[/] with a peak of [wheat1]{connection.PeakHeight}[/]");
                    n++;
                }

                MarkupLine($"Pruned [wheat1]{n}[/] connection{(n == 1 ? string.Empty : "s")}");
            });
        }
    }
}
