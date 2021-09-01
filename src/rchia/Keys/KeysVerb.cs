using System;
using System.Threading.Tasks;

using chia.dotnet;
using chia.dotnet.console;

using CommandLine;

namespace rchia.Keys
{
    [Verb("keys", HelpText = "Manage your keys\nRequires a wallet or daemon endpoint.")]
    internal sealed class KeysVerb
    {
        [Value(0, MetaName = "command", HelpText = "Commands:\nadd                  Add a private key by mnemonic\ndelete               Delete a key by its pk fingerprint in hex for\ndelete_all           Delete all private keys in keychain\n" +
                                                              "generate             Generates and adds a key to keychain\ngenerate_and_print   Generates but does NOT add to keychain\n" +
                                                              "show                 Displays all the keys in keychain\nsign                 Sign a message with a private key\nverify               Verify a signature with a pk")]
        public string? Command { get; set; }

        public async Task Add()
        {

        }

        public async Task Delete()
        {

        }

        public async Task DeleteAll()
        {

        }

        public async Task Generate()
        {

        }

        public async Task GenerateAndPrint()
        {

        }

        public async Task Show()
        {

        }

        public async Task Sign()
        {

        }

        public async Task Verify()
        {

        }
    }
}
