using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Bech32;
using rchia.Commands;

namespace rchia.Keys
{
    internal class KeysTasks : ConsoleTask<WalletProxy>
    {
        public KeysTasks(WalletProxy wallet, IConsoleMessage consoleMessage)
            : base(wallet, consoleMessage)
        {
        }

        public async Task Add(IEnumerable<string> mnemonic)
        {
            using var cts = new CancellationTokenSource(30000);
            var fingerprint = await Service.AddKey(mnemonic, true, cts.Token);

            ConsoleMessage.MarkupLine($"Added private key with public key fingerprint [bold]{fingerprint}[/]");
        }

        public async Task Delete(uint fingerprint)
        {
            using var cts = new CancellationTokenSource(30000);
            await Service.DeleteKey(fingerprint, cts.Token);

            ConsoleMessage.MarkupLine($"Deleted the key with fingerprint [bold]{fingerprint}[/]");
        }

        public async Task DeleteAll()
        {
            using var cts = new CancellationTokenSource(30000);
            await Service.DeleteAllKeys(cts.Token);
        }

        public async Task Generate()
        {
            using var cts = new CancellationTokenSource(30000);

            var mnemonic = await Service.GenerateMnemonic(cts.Token);
            var fingerprint = await Service.AddKey(mnemonic, true, cts.Token);

            ConsoleMessage.MarkupLine($"Added private key with public key fingerprint [bold]{fingerprint}[/]");
        }

        public async Task GenerateAndPrint()
        {
            using var cts = new CancellationTokenSource(30000);
            var mnemonic = await Service.GenerateMnemonic(cts.Token);

            ConsoleMessage.WriteLine("Generated private key. Mnemonic (24 secret words):");
            ConsoleMessage.MarkupLine($"[bold]{string.Join(' ', mnemonic)}[/]");
            ConsoleMessage.MarkupLine($"Note that this key has not been added to the keychain. Run [grey]rchia keys add {string.Join(' ', mnemonic)}[/] to do so.");
        }

        public async Task Show(bool showMnemonicSeed)
        {
            ConsoleMessage.Helpful("Showing all public keys derived from your private keys:");

            using var cts = new CancellationTokenSource(30000);
            var keys = await Service.GetPublicKeys(cts.Token);

            if (keys.Any())
            {
                var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);
                var bech32 = new Bech32M(NetworkPrefix);

                foreach (var fingerprint in keys)
                {
                    ConsoleMessage.NameValue("Fingerprint", fingerprint);
                    using var cts1 = new CancellationTokenSource(30000);
                    var (Fingerprint, Sk, Pk, FarmerPk, PoolPk, Seed) = await Service.GetPrivateKey(fingerprint, cts1.Token);

                    ConsoleMessage.NameValue("Master public key (m)", Pk);
                    ConsoleMessage.NameValue("Farmer public key(m/12381/8444/0/0)", FarmerPk);
                    ConsoleMessage.NameValue("Pool public key (m/12381/8444/1/0)", PoolPk);

                    // this isn't possible over rpc right now
                    //var address = bech32.PuzzleHashToAddress(HexBytes.FromHex(Sk));
                    // Console.WriteLine($"First wallet address", address(;

                    if (showMnemonicSeed)
                    {
                        ConsoleMessage.NameValue("Master private key (m)", Sk);

                        // this isn't possible over rpc right now
                        //Console.WriteLine($"First wallet secret key (m/12381/8444/2/0)", Sk(;

                        ConsoleMessage.NameValue("  Mnemonic seed (24 secret words)", Seed);
                    }
                }
            }
            else
            {
                ConsoleMessage.Warning("There are no saved private keys");
            }
        }
    }
}
