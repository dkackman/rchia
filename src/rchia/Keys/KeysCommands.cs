using rchia.Commands;

namespace rchia.Keys;

[Command("keys", Description = "Manage your keys\nRequires a wallet or daemon endpoint.")]
internal sealed class KeysCommands
{
    [Command("add", Description = "Add a private key by mnemonic")]
    public AddKeyCommand Add { get; init; } = new();

    [Command("delete", Description = "Delete a key by its pk fingerprint in hex form")]
    public DeleteKeyCommand Delete { get; init; } = new();

    [Command("generate-and-print", Description = "Generates but does NOT add to keychain")]
    public GenerateAndPrintKeyCommand GenerateAndPrint { get; init; } = new();

    [Command("generate", Description = "Generates and adds a key to keychain")]
    public GenerateKeyCommand Generate { get; init; } = new();

    [Command("show", Description = "Displays all the keys in keychain")]
    public ShowKeysCommand Show { get; init; } = new();

    [Command("delete-all", Description = "Delete all private keys in keychain")]
    public DeleteAllKeys DeleteAll { get; init; } = new();

    [Command("keyring", Description = "Shows the status of the node's keyring.")]
    public KeyringCommand Keyring { get; init; } = new();

    [Command("unlock", Description = "Unlocks the node's keyring.")]
    public UnlockCommand Unlock { get; init; } = new();

    [Command("migrate", Description = "Migrate the node to keyring.")]
    public MigrateCommand Migrate { get; init; } = new();

    [Command("set-passphrase", Description = "Set the keyring passphrase.")]
    public SetPassphraseCommand Set { get; init; } = new();
}
