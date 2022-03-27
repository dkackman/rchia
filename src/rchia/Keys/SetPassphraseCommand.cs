using System.IO;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Keys
{
    internal sealed class SetPassphraseCommand : EndpointOptions
    {
        [Option("p", "passphrase-file", Description = "Optional file containing the current passphrase. If not set the user will be prompted.")]
        public string? PassphraseFile { get; init; }

        [Option("n", "new-passphrase-file", Description = "Optional file containing the new passphrase. If not set the user will be prompted.")]
        public string? NewPassphraseFile { get; init; }

        [Option("t", "hint", IsRequired = true, Description = "Passphrase hint")]
        public string Hint { get; init; } = string.Empty;

        [Option("e", "current-passphrase-empty", Description = "The current passpharse is not set/empty.")]
        public bool CurrentPassphraseEmpty { get; init; }

        [Option("s", "save", Description = "Save the passphrase")]
        public bool Save { get; init; }

        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Setting the keyring passphrase...",
                output => GetPassphrase(output),
                async (inputs, output) =>
          {
              using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(output, this);
              var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
              using var cts = new CancellationTokenSource(TimeoutMilliseconds);

              await proxy.SetKeyringPassphrase(inputs.currentPassphase, inputs.newPassphrase, Hint, Save, cts.Token);

              output.WriteOutput("status", "success", Verbose);
          });
        }

        private (string currentPassphase, string newPassphrase) GetPassphrase(ICommandOutput output)
        {
            var currentPassphase = CurrentPassphraseEmpty ? string.Empty
                : string.IsNullOrEmpty(PassphraseFile) ? output.PromptForSecret("Enter the current keyring [green]passphrase[/].")
                : File.ReadAllText(PassphraseFile).Trim();

            var newPassphrase = string.IsNullOrEmpty(NewPassphraseFile) ? output.PromptForSecret("Enter the new keyring [green]passphrase[/].")
                : File.ReadAllText(NewPassphraseFile).Trim();

            // if we are prompting the user for the password have them re-enter it
            if (string.IsNullOrEmpty(NewPassphraseFile) && newPassphrase != output.PromptForSecret("Re-enter the new keyring [green]passphrase[/]."))
            {
                throw new InvalidDataException("Entered passphrases must match!");
            }

            return (currentPassphase, newPassphrase);
        }
    }
}
