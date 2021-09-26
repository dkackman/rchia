using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using Spectre.Console;

namespace rchia.Show
{
    internal sealed class ConnectionsCommand : EndpointOptions
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            return await DoWork2("Retrieving connections...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.FullNode);
                var proxy = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var connections = await proxy.GetConnections(cts.Token);

                MarkupLine("[wheat1]Connections[/]");
                var table = new Table();
                table.AddColumn("[orange3]Type[/]");
                table.AddColumn("[orange3]IP[/]");
                table.AddColumn("[orange3]Ports[/]");
                table.AddColumn("[orange3]NodeID[/]");
                table.AddColumn("[orange3]Last Connect[/]");
                table.AddColumn("[orange3]Up[/]");
                table.AddColumn("[orange3]Down[/]");
                table.AddColumn("[orange3]Height[/]");
                table.AddColumn("[orange3]Hash[/]");

                foreach (var c in connections)
                {
                    var id = Verbose ? c.NodeId : c.NodeId.Substring(2, 10) + "...";
                    var up = c.BytesRead ?? 0;
                    var down = c.BytesWritten ?? 0;
                    var ports = $"{c.PeerPort}/{c.PeerServerPort}";
                    var height = c.PeakHeight.HasValue ? c.PeakHeight.Value.ToString() : "na";
                    var hash = string.IsNullOrEmpty(c.PeakHash) ? "no info" : Verbose ? c.PeakHash : c.PeakHash.Substring(2, 10) + "...";

                    table.AddRow(c.Type.ToString(), c.PeerHost, ports, id, $"{c.LastMessageDateTime.ToLocalTime():MMM dd HH:mm}", up.ToBytesString("N1"), down.ToBytesString("N1"), height, hash);
                }

                AnsiConsole.Render(table);
            });
        }
    }
}
