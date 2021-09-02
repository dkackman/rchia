using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;
using rchia.Bech32;

namespace rchia.Keys
{
    internal class KeysTasks : ConsoleTask<WalletProxy>
    {
        public KeysTasks(WalletProxy wallet, IConsoleMessage consoleMessage)
            : base(wallet, consoleMessage)
        {
        }

        public async Task<uint> Add(IEnumerable<string> mnemonic)
        {
            using var cts = new CancellationTokenSource(1000);
            return await Service.AddKey(mnemonic, true, cts.Token);
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
            using var cts = new CancellationTokenSource(1000);
            var mnemonic = await Service.GenerateMnemonic(cts.Token);

            Console.WriteLine("Generating private key. Mnemonic (24 secret words):");
            Console.WriteLine(string.Join(' ', mnemonic));
            Console.WriteLine("Note that this key has not been added to the keychain. Run chia keys add");
        }

        public async Task Show()
        {
            Console.WriteLine("Showing all public keys derived from your private keys:");

            using var cts = new CancellationTokenSource(5000);
            var keys = await Service.GetPublicKeys(cts.Token);

            if (keys.Any())
            {
                var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);
                Bech32M.AddressPrefix = NetworkPrefix;

                foreach (var fingerprint in keys.Take(1))
                {
                    Console.WriteLine($"Fingerprint: {fingerprint}");
                    using var cts1 = new CancellationTokenSource(5000);
                    var (Fingerprint, Sk, Pk, FarmerPk, PoolPk, Seed) = await Service.GetPrivateKey(fingerprint, cts1.Token);

                    Console.WriteLine($"Master public key (m): {Pk}");
                    Console.WriteLine($"Farmer public key(m/12381/8444/0/0) :{FarmerPk}");
                    Console.WriteLine($"Pool public key (m/12381/8444/1/0): :{PoolPk}");

                    var address = Bech32M.PuzzleHashToAddress(HexBytes.FromHex(Sk));

                    Console.WriteLine($"First wallet address:: {address}");
                }
            }
            else
            {
                Console.WriteLine("There are no saved private keys");
            }
        }

        public async Task Sign()
        {

        }

        public async Task Verify()
        {

        }
    }
}
