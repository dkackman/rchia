using System;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Keys;

internal sealed class DeleteKeyCommand : WalletCommand
{
    [Option("f", "force", Default = false, Description = "Delete the key without prompting for confirmation")]
    public bool Force { get; init; }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Deleting key...", async output =>
        {
            if (Fingerprint is null || Fingerprint == 0)
            {
                throw new InvalidOperationException($"{Fingerprint} is not a valid wallet fingerprint");
            }

            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Farmer);

            if (output.Confirm($"Deleting a key CANNOT be undone.\nAre you sure you want to delete key {Fingerprint} from [red]{rpcClient.Endpoint.Uri}[/]?", Force))
            {
                var proxy = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                await proxy.DeleteKey(Fingerprint.Value, cts.Token);

                output.WriteOutput("deleted", Fingerprint.Value.ToString(), Verbose);
            }
        });
    }
}
