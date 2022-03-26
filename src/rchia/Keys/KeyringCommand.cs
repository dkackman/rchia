using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Keys
{
    internal sealed class KeyringCommand : EndpointOptions
    {
        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Retrieving keyring info...", async output =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(output, this);
                var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
                using var cts = new CancellationTokenSource(TimeoutMilliseconds);

                var keyring = await proxy.GetKeyringStatus(cts.Token);

                output.WriteOutput(keyring);
            });
        }
    }
}
