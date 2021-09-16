using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Bech32;
using rchia.Commands;
using Spectre.Console;

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
                ConsoleMessage.NameValue("Fingerprint", fingerprint);

                using var cts1 = new CancellationTokenSource(30000);
                _ = Service.LogIn(fingerprint, false, cts1.Token);
                var wallets = await Service.GetWallets(cts1.Token);

                var table = new Table();
                table.AddColumn("Id");
                table.AddColumn("Name");
                table.AddColumn("Type");

                foreach (var wallet in wallets)
                {
                    table.AddRow(wallet.Id.ToString(), wallet.Name, $"[green]{wallet.Type}[/]");
                }

                AnsiConsole.Render(table);
            }
        }

        public async Task Show()
        {
            using var cts = new CancellationTokenSource(30000);

            var (GenesisInitialized, Synced, Syncing) = await Service.GetSyncStatus(cts.Token);
            var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);
            var height = await Service.GetHeightInfo(cts.Token);

            ConsoleMessage.NameValue("Wallet height", await Service.GetHeightInfo(cts.Token));

            var synced = await Service.GetSyncStatus(cts.Token);
            ConsoleMessage.NameValue("Sync status", synced.Synced ? "Synced" : "Not synced");
            ConsoleMessage.NameValue("Wallet height", await Service.GetHeightInfo(cts.Token));
            ConsoleMessage.NameValue("Fingerprint", Service.Fingerprint);

            var wallets = await Service.GetWallets(cts.Token);
            if (!wallets.Any())
            {
                ConsoleMessage.Warning($"There are no wallets for a this public key {Service.Fingerprint}");
                return;
            }

            foreach (var summary in wallets)
            {
                var newWallet = new chia.dotnet.Wallet(summary.Id, Service);
                var (ConfirmedWalletBalance, UnconfirmedWalletBalance, SpendableBalance, PendingChange, MaxSendAmount, UnspentCoinCount, PendingCoinRemovalCount) = await newWallet.GetBalance(cts.Token);

                AnsiConsole.MarkupLine($"Wallet ID [bold]{summary.Id}[/] of type [green]{summary.Type}[/] '{summary.Name}'");
                ConsoleMessage.NameValue("   -Total Balance", $"{ConfirmedWalletBalance.AsChia()} {NetworkPrefix}");
                ConsoleMessage.NameValue("   -Pending Total Balance", $"{UnconfirmedWalletBalance.AsChia()} {NetworkPrefix}");
                ConsoleMessage.NameValue("   -Spendable", $"{SpendableBalance.AsChia()} {NetworkPrefix}");
                if (ConsoleMessage.Verbose)
                {
                    ConsoleMessage.NameValue("   -Pending Changee", $"{PendingChange.AsChia()} {NetworkPrefix}");
                    ConsoleMessage.NameValue("   -Max Spend Amount", $"{MaxSendAmount.AsChia()} {NetworkPrefix}");
                    ConsoleMessage.NameValue("   -Unspent Coin Count", $"{UnspentCoinCount}");
                    ConsoleMessage.NameValue("   -Pending Coin Removal Count", $"{PendingCoinRemovalCount}");
                }
            }
        }

        public async Task DeleteUnconfirmedTransactions(uint id)
        {
            using var cts = new CancellationTokenSource(30000);

            var wallet = new chia.dotnet.Wallet(id, Service);
            await wallet.DeleteUnconfirmedTransactions(cts.Token);

            AnsiConsole.MarkupLine($"Successfully deleted all unconfirmed transactions for wallet id [bold]{id}[/]");
        }

        public async Task GetAddress(uint id, bool newAddress)
        {
            using var cts = new CancellationTokenSource(30000);

            var wallet = new chia.dotnet.Wallet(id, Service);
            var address = await wallet.GetNextAddress(newAddress, cts.Token);

            AnsiConsole.MarkupLine($"[yellow]{address}[/]");
        }

        public async Task GetTransaction(string txId)
        {
            using var cts = new CancellationTokenSource(30000);
            var tx = await Service.GetTransaction(txId, cts.Token);
            var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);
            PrintTransaction(tx, NetworkPrefix);
        }

        public async Task GetTransactions(uint id, uint start, uint? count)
        {
            using var cts = new CancellationTokenSource(30000);

            var wallet = new chia.dotnet.Wallet(id, Service);
            var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);

            if (count is null)
            {
                count = await wallet.GetTransactionCount(cts.Token);
            }

            var transactions = await wallet.GetTransactions(start, count.Value - start, cts.Token);
            if (transactions.Any())
            {
                foreach (var tx in transactions)
                {
                    PrintTransaction(tx, NetworkPrefix);
                }
                var c = transactions.Count();
                ConsoleMessage.Message($"{c} transaction{(c > 0 ? "s" : string.Empty)}");
            }
            else
            {
                ConsoleMessage.Warning("There are no transactions to this address");
            }
        }

        private void PrintTransaction(TransactionRecord tx, string prefix)
        {
            using var cts = new CancellationTokenSource(30000);
            ConsoleMessage.NameValue("Transaction", tx.Name);
            if (tx.Confirmed)
            {
                ConsoleMessage.NameValue("Status", "Confirmed");
            }
            else if (tx.IsInMempool)
            {
                ConsoleMessage.NameValue($"Status", "In mempool");
            }
            else
            {
                ConsoleMessage.NameValue("Status", "Pending");
            }

            var (verb, color) = tx.Sent > 0 ? ("sent", "red") : ("received", "green");
            ConsoleMessage.NameValue($"Amount {verb}", $"[{color}]{tx.Amount.AsChia()} {prefix}[/]");
            var bech32 = new Bech32M(prefix);
            ConsoleMessage.NameValue("To address", bech32.PuzzleHashToAddress(tx.ToPuzzleHash));
            ConsoleMessage.NameValue("Created at", tx.CreatedAtDateTime.ToLocalTime());
        }

        public async Task Send(uint id, string address, decimal amount, decimal fee)
        {
            using var cts = new CancellationTokenSource(30000);
            var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);
            var wallet = new chia.dotnet.Wallet(id, Service);
            var tx = await wallet.SendTransaction(address, amount.ToMojo(), fee.ToMojo(), cts.Token);

            PrintTransaction(tx, NetworkPrefix);

            ConsoleMessage.Helpful($"Do 'rchia wallet get-transaction -tx {tx.TransactionId}' to get status");
        }
    }
}
