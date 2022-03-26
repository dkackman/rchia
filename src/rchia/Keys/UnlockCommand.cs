using System;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Keys
{
    internal sealed class UnlockCommand : EndpointOptions
    {
        [Argument(0, Name = "passphrase", Description = "The keyring passphrase")]
        public string Passphrase { get; init; } = string.Empty;

        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Unlocking the keyring...", async output =>
            {
                if (string.IsNullOrEmpty(Passphrase))
                {
                    throw new InvalidOperationException("Passphrase cannot be empty");
                }

                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(output, this);
                var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
                using var cts = new CancellationTokenSource(TimeoutMilliseconds);

                await proxy.UnlockKeyring(Passphrase, cts.Token);

                output.WriteOutput("status", "success", Verbose);
            });
        }
    }
}
