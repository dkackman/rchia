using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Keys;

internal sealed class GenerateAndPrintKeyCommand : EndpointOptions
{
    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Generating a new key....", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);

            var proxy = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var mnemonic = await proxy.GenerateMnemonic(cts.Token);

            output.WriteLine("Generated private key. Mnemonic (24 secret words):");

            output.WriteOutput(mnemonic);

            output.MarkupLine($"Note that this key has not been added to the keychain. Run '[grey]rchia keys add {string.Join(' ', mnemonic)}[/]' to do so.");
        });
    }
}
