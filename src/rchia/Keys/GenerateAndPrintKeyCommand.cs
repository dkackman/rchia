using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Keys
{
    internal sealed class GenerateAndPrintKeyCommand : EndpointOptions
    {
        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Generating a new key....", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Wallet);

                var proxy = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var mnemonic = await proxy.GenerateMnemonic(cts.Token);

                WriteLine("Generated private key. Mnemonic (24 secret words):");
                MarkupLine($"[wheat1]{string.Join(' ', mnemonic)}[/]");
                MarkupLine($"Note that this key has not been added to the keychain. Run '[grey]rchia keys add {string.Join(' ', mnemonic)}[/]' to do so.");
            });
        }
    }
}
