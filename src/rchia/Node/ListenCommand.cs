using System;
using System.Threading;
using System.Threading.Tasks;
using rchia.Commands;

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
            using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(output, this);

            if (!Promiscuous)
            {

            }

            using var cts = new CancellationTokenSource();
            rpcClient.BroadcastMessageReceived += (o, m) =>
            {
                output.WriteOutput(m);
            };

            while (!cts.IsCancellationRequested)
            {
                var key = await output.ReadKey(cts.Token);
                if (key is not null)
                {
                    cts.Cancel();
                }
                else
                {
                    await Task.Delay(10);
                }
            }
        });
    }
}
