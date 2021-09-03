using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;

namespace rchia.Wallet
{
    internal class WalletTasks : ConsoleTask<WalletProxy>
    {
        public WalletTasks(WalletProxy wallet, IConsoleMessage consoleMessage)
            : base(wallet, consoleMessage)
        {
        }

        public async Task List()
        {
            using var cts = new CancellationTokenSource(10000);
            var wallets = await Service.GetWallets(cts.Token);

            if (wallets.Any())
            {
                var fingerprints = await Service.GetPublicKeys(cts.Token);

                var zipped = wallets.Zip(fingerprints);

                Console.WriteLine($"{"Id",-5} {"Name",-20} {"Type",-20} {"Fingerprint",-20}");

                foreach (var (First, Second) in zipped)
                {
                    Console.WriteLine($"{First.Id,-5} {First.Name,-20} {First.Type,-20} {Second,-20}");
                }
            }
            else
            {
                Console.WriteLine("No wallets");
            }
        }

        public async Task Show(uint fingerprint)
        {
            using var cts = new CancellationTokenSource(10000);

            var fp = await Service.LogIn(fingerprint, true, cts.Token);

        }
    }
}
