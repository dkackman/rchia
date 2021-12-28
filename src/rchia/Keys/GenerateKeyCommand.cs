using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Keys;

internal sealed class GenerateKeyCommand : EndpointOptions
{
    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Generating a new key...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var proxy = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var mnemonic = await proxy.GenerateMnemonic(cts.Token);
            var fingerprint = await proxy.AddKey(mnemonic, true, cts.Token);

            output.WriteOutput("fingerprint", new Formattable<uint>(fingerprint, fp => $"{fp}"), Verbose);
        });
    }
}
