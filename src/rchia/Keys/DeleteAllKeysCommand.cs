using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Keys;

internal sealed class DeleteAllKeys : EndpointOptions
{
    [Option("f", "force", Description = "Delete all keys without prompting for confirmation")]
    public bool Force { get; init; }

    protected async override Task<bool> Confirm(ICommandOutput output)
    {
        await Task.CompletedTask;

        return output.Confirm($"Deleting all of your keys [wheat1]CANNOT[/] be undone.\nAre you sure you want to delete all keys?", Force);
    }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Deleting all keys....", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);

            var proxy = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            await proxy.DeleteAllKeys(cts.Token);

            output.WriteOutput("status", "Deleted all keys", Verbose);
        });
    }
}
