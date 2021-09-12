using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Bech32;
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
            using var cts = new CancellationTokenSource(30000);
            var keys = await Service.GetPublicKeys(cts.Token);
            if (!keys.Any())
            {
                throw new InvalidOperationException("No public keys found. You'll need to run `rchia keys generate`");
            }

            foreach (var fingerprint in keys)
            {
                Console.WriteLine($"Fingerprint: {fingerprint}");

                using var cts1 = new CancellationTokenSource(30000);
                _ = Service.LogIn(fingerprint, false, cts1.Token);
                var wallets = await Service.GetWallets(cts1.Token);

                Console.WriteLine($"{"Id",-5} {"Name",-20} {"Type",-20}"); ;

                foreach (var wallet in wallets)
                {
                    Console.WriteLine($"{wallet.Id,-5} {wallet.Name,-20} {wallet.Type,-20}"); ;
                }
                Console.WriteLine("");
            }
        }

        public async Task Show(uint id)
        {
            using var cts = new CancellationTokenSource(30000);

            var (GenesisInitialized, Synced, Syncing) = await Service.GetSyncStatus(cts.Token);
            var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);
            var height = await Service.GetHeightInfo(cts.Token);

            var wallets = await Service.GetWallets(cts.Token);

            Console.WriteLine($"Wallet height: {await Service.GetHeightInfo(cts.Token)}");
            var synced = await Service.GetSyncStatus(cts.Token);
            Console.WriteLine($"Sync status: {(synced.Synced ? "Synced" : "Not synced")}");
            Console.WriteLine($"Balances, fingerprint: {Service.Fingerprint}");

            foreach (var summary in wallets)
            {
                var newWallet = new chia.dotnet.Wallet(summary.Id, Service);
                var (ConfirmedWalletBalance, UnconfirmedWalletBalance, SpendableBalance, PendingChange, MaxSendAmount, UnspentCoinCount, PendingCoinRemovalCount) = await newWallet.GetBalance(cts.Token);

                Console.WriteLine($"Wallet ID {summary.Id} type {summary.Type} {summary.Name}");
                Console.WriteLine($"   -Total Balance: {ConfirmedWalletBalance.AsChia()} {NetworkPrefix}");
                Console.WriteLine($"   -Pending Total Balance: {UnconfirmedWalletBalance.AsChia()} {NetworkPrefix}");
                Console.WriteLine($"   -Spendable: {SpendableBalance.AsChia()} {NetworkPrefix}");
                if (ConsoleMessage.Verbose)
                {
                    Console.WriteLine($"   -Pending Changee: {PendingChange.AsChia()} {NetworkPrefix}");
                    Console.WriteLine($"   -Max Spend Amount: {MaxSendAmount.AsChia()} {NetworkPrefix}");
                    Console.WriteLine($"   -Unspent Coin Count: {UnspentCoinCount}");
                    Console.WriteLine($"   -Pending Coin Removal Count: {PendingCoinRemovalCount}");
                }
            }
        }

        public async Task DeleteUnconfirmedTransactions(uint id)
        {
            using var cts = new CancellationTokenSource(30000);

            var wallet = new chia.dotnet.Wallet(id, Service);
            await wallet.DeleteUnconfirmedTransactions(cts.Token);

            Console.WriteLine($"Successfully deleted all unconfirmed transactions for wallet id {id}");
        }

        public async Task GetAddress(uint id, bool newAddress)
        {
            using var cts = new CancellationTokenSource(30000);

            var wallet = new chia.dotnet.Wallet(id, Service);
            var address = await wallet.GetNextAddress(newAddress, cts.Token);

            Console.WriteLine(address);
        }

        public async Task GetTransaction(string txId)
        {
            using var cts = new CancellationTokenSource(30000);
            var tx = await Service.GetTransaction(txId, cts.Token);
            var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);
            PrintTransaction(tx, NetworkPrefix);
        }


        public async Task GetTransactions(uint id)
        {
            using var cts = new CancellationTokenSource(30000);

            var wallet = new chia.dotnet.Wallet(id, Service);
            var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);
            var transactions = await wallet.GetTransactions(cts.Token);

            if (transactions.Any())
            {
                foreach (var tx in transactions)
                {
                    PrintTransaction(tx, NetworkPrefix);
                }
            }
            else
            {
                Console.WriteLine("There are no transactions to this address");
            }
        }

        private static void PrintTransaction(TransactionRecord tx, string prefix)
        {
            using var cts = new CancellationTokenSource(30000);
            Console.WriteLine($"Transaction {tx.Name}");
            if (tx.Confirmed)
            {
                Console.WriteLine($"Status: Confirmed");
            }
            else if (tx.IsInMempool)
            {
                Console.WriteLine($"Status: In mempool");
            }
            else
            {
                Console.WriteLine($"Status: Pending");
            }

            Console.WriteLine($"Amount {(tx.Sent > 0 ? "sent" : "received")} {tx.Amount.AsChia()} {prefix}");
            var bech32 = new Bech32M(prefix);
            Console.WriteLine($"To address: {bech32.PuzzleHashToAddress(tx.ToPuzzleHash)}");
            Console.WriteLine($"Created at: {tx.CreatedAtDateTime.ToLocalTime()}");
            Console.WriteLine("");
        }


        public async Task Send(uint id, string address, decimal amount, decimal fee)
        {
            Console.WriteLine("Submitting transaction...");

            using var cts = new CancellationTokenSource(30000);
            var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);
            var wallet = new chia.dotnet.Wallet(id, Service);
            var tx = await wallet.SendTransaction(address, amount.ToMojo(), fee.ToMojo(), cts.Token);

            PrintTransaction(tx, NetworkPrefix);

            Console.WriteLine($"Do 'rchia wallet get-transaction -tx {tx.TransactionId}' to get status");
        }
    }
}
