using System;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Keys;

internal sealed class DeleteKeyCommand : WalletCommand
{
    [Option("f", "force", Description = "Delete the key without prompting for confirmation")]
    public bool Force { get; init; }

    protected async override Task<bool> Confirm(ICommandOutput output)
    {
        await Task.CompletedTask;

        return output.Confirm($"Deleting a key CANNOT be undone.\nAre you sure you want to delete key {Fingerprint}?", Force);
    }

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
            var proxy = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            await proxy.DeleteKey((uint)Fingerprint.Value, cts.Token);

            output.WriteOutput("deleted", new Formattable<uint>((uint)Fingerprint.Value, fp => $"{fp}"), Verbose);
        });
    }
}
