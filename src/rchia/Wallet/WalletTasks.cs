using System;
using System.Collections.Generic;
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
            var wallets = await GetAllWalletInfo();

            Console.WriteLine($"{"Id",-5} {"Name",-20} {"Type",-20} {"Fingerprint",-20}");

            foreach (var wallet in wallets)
            {
                Console.WriteLine($"{wallet.Wallet.Id,-5} {wallet.Wallet.Name,-20} {wallet.Wallet.Type,-20} {wallet.Fingerprint,-20}");
            }
        }

        private async Task<IEnumerable<(WalletInfo Wallet, uint Fingerprint)>> GetAllWalletInfo()
        {
            using var cts = new CancellationTokenSource(10000);
            var wallets = await Service.GetWallets(cts.Token);

            if (wallets.Any())
            {
                var fingerprints = await Service.GetPublicKeys(cts.Token);

                return wallets.Zip(fingerprints);
            }

            throw new InvalidOperationException("No wallets");
        }

        public async Task Show(uint fingerprint)
        {
            using var cts = new CancellationTokenSource(20000);

            var fp = await Service.LogIn(fingerprint, true, cts.Token);
            var (GenesisInitialized, Synced, Syncing) = await Service.GetSyncStatus(cts.Token);
            var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);
            var height = await Service.GetHeightInfo(cts.Token);

            var wallets = await GetAllWalletInfo();
            var walletInfo = wallets.First(info => info.Fingerprint == fingerprint);

            var wallet = new chia.dotnet.Wallet(walletInfo.Wallet.Id, Service);
        }

        public async Task Show(int id)
        {
            using var cts = new CancellationTokenSource(10000);

            var fingerprints = await Service.GetPublicKeys(cts.Token);

            var skipped = fingerprints.Skip(id - 1);
            if (!skipped.Any())
            {
                throw new InvalidOperationException($"No wallet with an id of {id}");
            }

            await Show(skipped.First());
        }

        public async Task DeleteUnconfirmedTransactions(uint fingerprint)
        {
            using var cts = new CancellationTokenSource(20000);

            var wallets = await GetAllWalletInfo();
            var walletInfo = wallets.First(info => info.Fingerprint == fingerprint);

            var wallet = new chia.dotnet.Wallet(walletInfo.Wallet.Id, Service);
            await wallet.DeleteUnconfirmedTransactions(cts.Token);

            Console.WriteLine($"Successfully deleted all unconfirmed transactions for wallet id {walletInfo.Wallet.Id} on key {fingerprint}");
        }

        public async Task DeleteUnconfirmedTransactions(int id)
        {
            using var cts = new CancellationTokenSource(20000);

            var wallet = new chia.dotnet.Wallet((uint)id, Service);
            await wallet.DeleteUnconfirmedTransactions(cts.Token);

            Console.WriteLine($"Successfully deleted all unconfirmed transactions for wallet id {id}");
        }

        public async Task GetAddress(uint fingerprint)
        {
            using var cts = new CancellationTokenSource(20000);

            var wallets = await GetAllWalletInfo();
            var walletInfo = wallets.First(info => info.Fingerprint == fingerprint);

            var wallet = new chia.dotnet.Wallet(walletInfo.Wallet.Id, Service);
            var address = await wallet.GetNextAddress(false, cts.Token);

            Console.WriteLine(address);
        }

        public async Task GetAddress(int id)
        {
            using var cts = new CancellationTokenSource(20000);

            var wallet = new chia.dotnet.Wallet((uint)id, Service);
            var address = await wallet.GetNextAddress(false, cts.Token);

            Console.WriteLine(address);
        }
    }
}
