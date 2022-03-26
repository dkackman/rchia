using System;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Keys
{
    internal sealed class SetPassphraseCommand : EndpointOptions
    {
        [Argument(0, Name = "passphrase", Description = "The keyring passphrase")]
        public string Passphrase { get; init; } = string.Empty;

        [Argument(1, Name = "new-passphrase", Description = "The keyring passphrase")]
        public string NewPassphrase { get; init; } = string.Empty;

        [Argument(2, Name = "hint", Description = "The passphrase hint")]
        public string Hint { get; init; } = string.Empty;

        [Option("s", "save", Description = "Save the passphrase")]
        public bool Save { get; init; }

        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Unlocking the keyring...", async output =>
            {
                if (string.IsNullOrEmpty(NewPassphrase))
                {
                    throw new InvalidOperationException("Passphrase cannot be empty");
                }

                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(output, this);
                var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
                using var cts = new CancellationTokenSource(TimeoutMilliseconds);

                await proxy.SetKeyringPassphrase(Passphrase, NewPassphrase, Hint, Save, cts.Token);

                output.WriteOutput("status", "success", Verbose);
            });
        }
    }
}
