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
        [Argument(0, Name = "command", Description = "Commands:\nadd                  Add a private key by mnemonic\ndelete               Delete a key by its pk fingerprint in hex for\ndelete_all           Delete all private keys in keychain\n" +
                                                              "generate             Generates and adds a key to keychain\ngenerate_and_print   Generates but does NOT add to keychain\n" +
                                                              "show                 Displays all the keys in keychain\nsign                 Sign a message with a private key\nverify               Verify a signature with a pk")]
        public string? Command { get; set; }


        public override Task<int> Run()
        {
            throw new NotImplementedException();
        }
    }
}
