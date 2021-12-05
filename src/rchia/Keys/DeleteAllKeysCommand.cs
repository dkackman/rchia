using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Keys;

internal sealed class DeleteAllKeys : EndpointOptions
{
    [Option("f", "force", Default = false, Description = "Delete all keys without prompting for confirmation")]
    public bool Force { get; init; }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Deleting all keys....", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);

            if (output.Confirm($"Deleting all of your keys [wheat1]CANNOT[/] be undone.\nAre you sure you want to delete all keys from [red]{rpcClient.Endpoint.Uri}[/]?", Force))
            {
                var proxy = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                await proxy.DeleteAllKeys(cts.Token);

                output.MarkupLine("Deleted all keys");
            }
        });
    }
}
