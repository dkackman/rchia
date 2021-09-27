using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Keys
{
    internal sealed class GenerateKeyCommand : EndpointOptions
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            return await DoWorkAsync("Generating a new key...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Wallet);

                var proxy = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var mnemonic = await proxy.GenerateMnemonic(cts.Token);
                var fingerprint = await proxy.AddKey(mnemonic, true, cts.Token);

                MarkupLine($"Added private key with public key fingerprint [wheat1]{fingerprint}[/]");
                MarkupLine($"[wheat1]{string.Join(' ', mnemonic)}[/]");
            });
        }
    }
}
