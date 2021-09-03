using rchia.Commands;

namespace rchia.Keys
{
    [Command("keys", Description = "Manage your keys\nRequires a wallet or daemon endpoint.")]
    internal sealed class KeysCommand
    {
        [Command("add", Description = "Add a private key by mnemonic")]
        public AddKeyCommand Add { get; set; } = new();

        [Command("delete", Description = "Delete a key by its pk fingerprint in hex form")]
        public DeleteKeyCommand Delete { get; set; } = new();

        [Command("generate-and-print", Description = "Generates but does NOT add to keychain")]
        public GenerateAndPrintKeyCommand GenerateAndPrint { get; set; } = new();

        [Command("generate", Description = "Generates and adds a key to keychain")]
        public GenerateKeyCommand Generate { get; set; } = new();

        [Command("show", Description = "Displays all the keys in keychain")]
        public ShowKeysCommand Show { get; set; } = new();

        [Command("delete-all", Description = "Delete all private keys in keychain")]
        public DeleteAllKeys DeleteAll { get; set; } = new();
    }
}
