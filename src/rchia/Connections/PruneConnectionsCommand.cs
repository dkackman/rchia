﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Connections
{
    internal sealed class PruneConnectionsCommand : EndpointOptions
    {
        [Argument(0, Name = "blocks", Default = 10, Description = "Prune nodes that are this many blocks behind the sync tip height")]
        public ulong Blocks { get; init; } = 10;

        [CommandTarget]
        public async Task<int> Run()
        {
            if (Blocks < 1)
            {
                throw new InvalidOperationException("A number of blocks must be provided");
            }

            return await DoWorkAsync("Pruning connections...", async output =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.FullNode);
                var fullNode = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var state = await fullNode.GetBlockchainState(cts.Token);
                if (Blocks > state.Sync.SyncProgressHeight)
                {
                    throw new InvalidOperationException("Blocks offset is greater than synced height. Aborting.");
                }

                var maxHeight = state.Sync.SyncProgressHeight - Blocks;
                output.MarkupLine($"Pruning connections that with a peak less than [wheat1]{maxHeight}[/]");

                var connections = await fullNode.GetConnections(cts.Token);
                // only prune other full nodes, not famers, harvesters, and wallets etc
                var n = 0;
                var list = new List<string>();
                foreach (var connection in connections.Where(c => c.Type == NodeType.FULL_NODE && c.PeakHeight < maxHeight))
                {
                    using var cts1 = new CancellationTokenSource(TimeoutMilliseconds);

                    await fullNode.CloseConnection(connection.NodeId, cts1.Token);
                    list.Add($"{connection.PeerHost}:{connection.PeerServerPort}");
                    n++;
                }
                output.WriteOutput(list);
                output.MarkupLine($"Pruned [wheat1]{n}[/] connection{(n == 1 ? string.Empty : "s")}");
            });
        }
    }
}
