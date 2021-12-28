using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Connections;

internal sealed class ListConnectionsCommand : EndpointOptions
{
    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving connections...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.FullNode);
            var proxy = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var connections = await proxy.GetConnections(cts.Token);

            if (Json)
            {
                output.WriteOutput(connections);
            }
            else
            {
                var table = from c in await proxy.GetConnections(cts.Token)
                            select new Dictionary<string, object?>()
                            {
                                { "Type", c.Type },
                                { "IP", c.PeerHost },
                                { "Ports", $"{c.PeerPort}/{c.PeerServerPort}" },
                                { "NodeID", Verbose ? c.NodeId : string.Concat(c.NodeId.AsSpan(2, 10), "...") },
                                { "Last Connect", $"{c.LastMessageDateTime.ToLocalTime():MMM dd HH:mm}" },
                                { "Up", (c.BytesRead ?? 0).ToBytesString("N1") },
                                { "Down", (c.BytesWritten ?? 0).ToBytesString("N1") },
                                { "Height", c.PeakHeight },
                                { "Hash", string.IsNullOrEmpty(c.PeakHash) ? "no info" : Verbose ? c.PeakHash : string.Concat(c.PeakHash.AsSpan(2, 10), "...") }
                            };

                output.WriteOutput(table);
            }
        });
    }
}
