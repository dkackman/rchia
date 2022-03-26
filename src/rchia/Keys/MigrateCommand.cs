using System.IO;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Keys
{
    internal sealed class MigrateCommand : EndpointOptions
    {
        [Option("p", "passphrase-file", Description = "Optional file containing the passphrase. If not set the user will be prompted.")]
        public string? PassphraseFile { get; init; }

        [Option("t", "hint", IsRequired = true, Description = "Passphrase hint")]
        public string Hint { get; init; } = string.Empty;

        [Option("s", "save", Description = "Save the passphrase")]
        public bool Save { get; init; }

        [Option("c", "clean", Description = "Cleanup the legacy keyring")]
        public bool Clean { get; init; }

        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Migrating the keyring...", output => GetPassphrase(output), async (output, passphrase) =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(output, this);
                var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
                using var cts = new CancellationTokenSource(TimeoutMilliseconds);

                await proxy.MigrateKeyring(passphrase, Hint, Save, Clean, cts.Token);

                output.WriteOutput("status", "success", Verbose);
            });
        }

        private string GetPassphrase(ICommandOutput output)
        {
            if (!string.IsNullOrEmpty(PassphraseFile))
            {
                return File.ReadAllText(PassphraseFile).Trim();
            }

            var one = output.PromptSecret("Enter the new keyring [green]passphrase[/]?");
            var two = output.PromptSecret("Re-enter the new keyring [green]passphrase[/]?");

            if (one != two)
            {
                throw new InvalidDataException("Entered passphrases must match!");
            }

            return one;
        }
    }
}
