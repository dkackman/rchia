using System;
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
            using var cts = new CancellationTokenSource(20000);
            var fingerprint = await Service.AddKey(mnemonic, true, cts.Token);

            Console.WriteLine($"Added private key with public key fingerprint {fingerprint}");
        }

        public async Task Delete(uint fingerprint)
        {
            using var cts = new CancellationTokenSource(20000);
            await Service.DeleteKey(fingerprint, cts.Token);

            Console.WriteLine($"Deleted the key with fingerprint {fingerprint}");
        }

        public async Task DeleteAll()
        {
            using var cts = new CancellationTokenSource(20000);
            await Service.DeleteAllKeys(cts.Token);
        }

        public async Task Generate()
        {
            using var cts = new CancellationTokenSource(20000);
            Console.WriteLine("Generating private key.");

            var mnemonic = await Service.GenerateMnemonic(cts.Token);
            var fingerprint = await Service.AddKey(mnemonic, true, cts.Token);

            Console.WriteLine($"Added private key with public key fingerprint {fingerprint}");
        }

        public async Task GenerateAndPrint()
        {
            using var cts = new CancellationTokenSource(20000);
            var mnemonic = await Service.GenerateMnemonic(cts.Token);

            Console.WriteLine("Generating private key. Mnemonic (24 secret words):");
            Console.WriteLine(string.Join(' ', mnemonic));
            Console.WriteLine("Note that this key has not been added to the keychain. Run chia keys add");
        }

        public async Task Show(bool showMnemonicSeed)
        {
            Console.WriteLine("Showing all public keys derived from your private keys:");

            using var cts = new CancellationTokenSource(20000);
            var keys = await Service.GetPublicKeys(cts.Token);

            if (keys.Any())
            {
                var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);
                var bech32 = new Bech32M(NetworkPrefix);

                foreach (var fingerprint in keys.Take(1))
                {
                    Console.WriteLine($"Fingerprint: {fingerprint}");
                    using var cts1 = new CancellationTokenSource(20000);
                    var (Fingerprint, Sk, Pk, FarmerPk, PoolPk, Seed) = await Service.GetPrivateKey(fingerprint, cts1.Token);

                    Console.WriteLine($"Master public key (m): {Pk}");
                    Console.WriteLine($"Farmer public key(m/12381/8444/0/0): {FarmerPk}");
                    Console.WriteLine($"Pool public key (m/12381/8444/1/0): {PoolPk}");

                    // this isn't possible over rpc right now
                    //var address = bech32.PuzzleHashToAddress(HexBytes.FromHex(Sk));

                    // Console.WriteLine($"First wallet address: {address}");

                    if (showMnemonicSeed)
                    {
                        Console.WriteLine($"Master private key (m): {Sk}");

                        // this isn't possible over rpc right now
                        //Console.WriteLine($"First wallet secret key (m/12381/8444/2/0): {Sk}");

                        Console.WriteLine($"  Mnemonic seed (24 secret words):\n  {Seed}");
                    }
                }
            }
            else
            {
                Console.WriteLine("There are no saved private keys");
            }
        }
    }
}
