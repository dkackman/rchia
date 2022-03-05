using System;
using System.Threading;
using System.Threading.Tasks;
using rchia.Commands;
using chia.dotnet;

namespace rchia.Node;

internal sealed class ListenCommand : EndpointOptions
{
    [Option("p", "promiscuous", Description = "Include messages sent to other rchia instances")]
    public bool Promiscuous { get; init; }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Press any key to stop...", async output =>
        {
            if (!Promiscuous)
            {
                // messages get routed by the OriginService name
                // generate a unique name so we don't get other rchia instance owned responses
                ClientFactory.Initialize(Guid.NewGuid().ToString());
            }

            using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(output, this);
            using var cts = new CancellationTokenSource();
            rpcClient.BroadcastMessageReceived += (o, m) =>
            {
                if (Verbose)
                {
                    output.WriteLine(DateTime.Now.ToString());
                }
                output.WriteOutput(m);
                output.WriteLine("");
            };

            while (!cts.IsCancellationRequested)
            {
                if (await output.ReadKey(cts.Token) is not null)
                {
                    cts.Cancel();
                }
                else
                {
                    await Task.Delay(10, cts.Token);
                }
            }
        });
    }
}
