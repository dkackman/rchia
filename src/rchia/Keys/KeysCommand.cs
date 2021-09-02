using System;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Keys
{
    [Command("keys", Description = "Manage your keys\nRequires a wallet or daemon endpoint.")]
    internal sealed class KeysCommand : SharedOptions
    {
        [Command("add", Description = "Add a private key by mnemonic")]
        public AddKeyCommand Add { get; set; } = new();

        [Command("delete", Description = "Delete a key by its pk fingerprint in hex form")]
        public DeleteKeyCommand Delete { get; set; } = new();

        [Option("g", "generate-and-print", Description = "Generates but does NOT add to keychain")]
        public bool GenerateAndPrint { get; set; }

        [Option("s", "show", Description = "Displays all the keys in keychain")]
        public bool Show { get; set; }

        [Option("m", "show-mnemonic-seed", Default = false, Description = "Show the mnemonic seed of the keys")]
        public bool ShowMnemonicSeed { get; set; }

        [Option("a", "delete-all", Default = false, Description = "Delete all private keys in keychain")]
        public bool DeleteAll { get; set; }

        [Option("f", "force", Description = "Do not prompt before deleting keys")]
        public bool Force { get; set; }

        public override async Task<int> Run()
        {
            try
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Wallet);
                var wallet = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                var commands = new KeysTasks(wallet, this);

                if (GenerateAndPrint)
                {
                    await commands.GenerateAndPrint();

                }
                else if (Show)
                {
                    await commands.Show(ShowMnemonicSeed);
                }
                else if (DeleteAll)
                {
                    if (Confirm("Deleting all of your keys CANNOT be undone.", "Are you sure you want to delete all of your keys?", Force))
                    {
                        Message("Deleting all keys...");
                        await commands.DeleteAll();
                        Message("All keys deleted.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Unrecognized command");
                }

                return 0;
            }
            catch (Exception e)
            {
                Message(e);

                return -1;
            }
        }
    }
}
