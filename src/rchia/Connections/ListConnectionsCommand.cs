using System;
using System.Collections.Generic;
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
                var table = new List<IDictionary<string, string>>();

                foreach (var c in await proxy.GetConnections(cts.Token))
                {
                    var row = new Dictionary<string, string>
                    {
                        { "Type", c.Type.ToString() },
                        { "IP", c.PeerHost },
                        { "Ports", $"{c.PeerPort}/{c.PeerServerPort}" },
                        { "NodeID", Verbose ? c.NodeId : string.Concat(c.NodeId.AsSpan(2, 10), "...") },
                        { "Last Connect", $"{c.LastMessageDateTime.ToLocalTime():MMM dd HH:mm}" },
                        { "Up", (c.BytesRead ?? 0).ToBytesString("N1") },
                        { "Down", (c.BytesWritten ?? 0).ToBytesString("N1") },
                        { "Height", c.PeakHeight.HasValue ? c.PeakHeight.Value.ToString() : "na" },
                        { "Hash", string.IsNullOrEmpty(c.PeakHash) ? "no info" : Verbose ? c.PeakHash : string.Concat(c.PeakHash.AsSpan(2, 10), "...") }
                    };

                    table.Add(row);
                }

                output.WriteOutput(table);
            }
        });
    }
}
