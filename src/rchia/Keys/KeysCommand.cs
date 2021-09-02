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
        //delete               Delete a key by its pk fingerprint in hex for\ndelete_all           Delete all private keys in keychain\n" +
        //                                                      "generate             Generates and adds a key to keychain\ngenerate_and_print   Generates but does NOT add to keychain\n" +
        //                                                      "show                 Displays all the keys in keychain\nsign                 Sign a message with a private key\nverify               Verify a signature with a pk")]
        [Command("add", Description = "Add a private key by mnemonic")]
        public AddKeysCommand Add { get; set; } = new();

        [Option('g', "generate-and-print", Description = "Generates but does NOT add to keychain")]
        public bool GenerateAndPrint { get; set; }

        [Option('s', "show", Description = "Displays all the keys in keychain")]
        public bool Show { get; set; }

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
                    await commands.Show();
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
