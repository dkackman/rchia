﻿using System.IO;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Keys
{
    internal sealed class UnlockCommand : EndpointOptions
    {
        [Option("p", "passphrase-file", Description = "Optional file containing the passphrase. If not set the user will be prompted.")]
        public string? PassphraseFile { get; init; }

        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Unlocking the keyring...", 
                output => GetPassphrase(output), 
                async (passphrase, output) =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(output, this);
                var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
                using var cts = new CancellationTokenSource(TimeoutMilliseconds);

                await proxy.UnlockKeyring(passphrase, cts.Token);

                output.WriteOutput("status", "success", Verbose);
            });
        }

        private string GetPassphrase(ICommandOutput output)
        {
            if (!string.IsNullOrEmpty(PassphraseFile))
            {
                return File.ReadAllText(PassphraseFile).Trim();
            }

            return output.PromptForSecret("Enter the keyring [green]passphrase[/]?");
        }
    }
}
