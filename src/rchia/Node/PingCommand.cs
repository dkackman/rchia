using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Node
{
    internal sealed class PingCommand : EndpointOptions
    {
        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Pinging the daemon...", async output =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(output, this);
                var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);

                var stopWatch = Stopwatch.StartNew();
                await proxy.Ping();
                stopWatch.Stop();

                var result = new Dictionary<string, string>()
                {
                    { "response_time", $"{stopWatch.ElapsedMilliseconds / 1000.0:N2}"}
                };
                output.WriteOutput(result);
            });
        }
    }
}
