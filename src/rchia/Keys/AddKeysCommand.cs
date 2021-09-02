using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Keys
{
    internal sealed class AddKeysCommand : SharedOptions
    {
        [Argument(0, Name = "mnemonic", Description = "The 24 word mnemonic key phrase")]
        public List<string> Mnemonic { get; set; } = new List<string>();

        public override async Task<int> Run()
        {
            try
            {
                if (Mnemonic.Count != 24)
                {
                    throw new InvalidOperationException("Exactly 24 words are required in the mnenomic passphrase");
                }

                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Wallet);
                var wallet = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                var commands = new KeysTasks(wallet, this);

                var fingerprint = await commands.Add(Mnemonic);
                Console.WriteLine($"Added private key with public key fingerprint {fingerprint}");

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
