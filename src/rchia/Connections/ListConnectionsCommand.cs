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

            var table = new List<IDictionary<string, string>>();

            foreach (var c in await proxy.GetConnections(cts.Token))
            {
                var row = new Dictionary<string, string>();

                row.Add("Type", c.Type.ToString());
                row.Add("IP", c.PeerHost);
                row.Add("Ports", $"{c.PeerPort}/{c.PeerServerPort}");
                row.Add("NodeID", Verbose ? c.NodeId : c.NodeId.Substring(2, 10) + "...");
                row.Add("Last Connect", $"{c.LastMessageDateTime.ToLocalTime():MMM dd HH:mm}");
                row.Add("Up", (c.BytesRead ?? 0).ToBytesString("N1"));
                row.Add("Down", (c.BytesWritten ?? 0).ToBytesString("N1"));
                row.Add("Height", c.PeakHeight.HasValue ? c.PeakHeight.Value.ToString() : "na");
                row.Add("Hash", string.IsNullOrEmpty(c.PeakHash) ? "no info" : Verbose ? c.PeakHash : c.PeakHash.Substring(2, 10) + "...");

                table.Add(row);
            }

            output.WriteOutput(table);
        });
    }
}
