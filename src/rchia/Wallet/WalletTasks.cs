﻿using System;
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
        public WalletTasks(WalletProxy wallet, IConsoleMessage consoleMessage, int timeoutSeconds)
            : base(wallet, consoleMessage, timeoutSeconds)
        {
        }

        public async Task List()
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var keys = await Service.GetPublicKeys(cts.Token);
            if (!keys.Any())
            {
                throw new InvalidOperationException("No public keys found. You'll need to run `rchia keys generate`");
            }

            foreach (var fingerprint in keys)
            {
                ConsoleMessage.NameValue("Fingerprint", fingerprint);

                using var cts1 = new CancellationTokenSource(TimeoutMilliseconds);
                _ = Service.LogIn(fingerprint, false, cts1.Token);
                var wallets = await Service.GetWallets(cts1.Token);

                var table = new Table();
                table.AddColumn("[orange3]Id[/]");
                table.AddColumn("[orange3]Name[/]");
                table.AddColumn("[orange3]Type[/]");

                foreach (var wallet in wallets)
                {
                    table.AddRow(wallet.Id.ToString(), wallet.Name, $"[green]{wallet.Type}[/]");
                }

                AnsiConsole.Render(table);
            }
        }

        public async Task Show()
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            var (GenesisInitialized, Synced, Syncing) = await Service.GetSyncStatus(cts.Token);
            var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);
            var height = await Service.GetHeightInfo(cts.Token);

            ConsoleMessage.NameValue("Wallet height", await Service.GetHeightInfo(cts.Token));

            var synced = await Service.GetSyncStatus(cts.Token);
            ConsoleMessage.NameValue("Sync status", synced.Synced ? "[green]Synced[/]" : "[red]Not synced[/]");
            ConsoleMessage.NameValue("Wallet height", await Service.GetHeightInfo(cts.Token));
            ConsoleMessage.NameValue("Fingerprint", Service.Fingerprint);

            var wallets = await Service.GetWallets(cts.Token);
            if (!wallets.Any())
            {
                ConsoleMessage.Warning($"There are no wallets for a this public key {Service.Fingerprint}");
                return;
            }

            ConsoleMessage.MarkupLine($"[orange3]Balances ({NetworkPrefix})[/]");
            var table = new Table();
            table.AddColumn("[orange3]Id[/]");
            table.AddColumn("[orange3]Type[/]");
            table.AddColumn("[orange3]Name[/]");
            table.AddColumn("[orange3]Total[/]");
            table.AddColumn("[orange3]Pending Total[/]");
            table.AddColumn("[orange3]Spendable[/]");

            foreach (var summary in wallets)
            {
                var newWallet = new chia.dotnet.Wallet(summary.Id, Service);
                var (ConfirmedWalletBalance, UnconfirmedWalletBalance, SpendableBalance, PendingChange, MaxSendAmount, UnspentCoinCount, PendingCoinRemovalCount) = await newWallet.GetBalance(cts.Token);

                table.AddRow(summary.Id.ToString(), summary.Name, $"[green]{summary.Type}[/]", ConfirmedWalletBalance.AsChia(), UnconfirmedWalletBalance.AsChia(), SpendableBalance.AsChia());

                //if (ConsoleMessage.Verbose)
                //{
                //    ConsoleMessage.NameValue("   -Pending Change", $"{PendingChange.AsChia()} {NetworkPrefix}");
                //    ConsoleMessage.NameValue("   -Max Spend Amount", $"{MaxSendAmount.AsChia()} {NetworkPrefix}");
                //    ConsoleMessage.NameValue("   -Unspent Coin Count", $"{UnspentCoinCount}");
                //    ConsoleMessage.NameValue("   -Pending Coin Removal Count", $"{PendingCoinRemovalCount}");
                //}
            }

            AnsiConsole.Render(table);
        }

        public async Task DeleteUnconfirmedTransactions(uint id)
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            var wallet = new chia.dotnet.Wallet(id, Service);
            await wallet.DeleteUnconfirmedTransactions(cts.Token);

            ConsoleMessage.MarkupLine($"Successfully deleted all unconfirmed transactions for wallet id [wheat1]{id}[/]");
        }

        public async Task GetAddress(uint id, bool newAddress)
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            var wallet = new chia.dotnet.Wallet(id, Service);
            var address = await wallet.GetNextAddress(newAddress, cts.Token);

            ConsoleMessage.MarkupLine($"[yellow]{address}[/]");
        }

        public async Task GetTransaction(string txId)
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var tx = await Service.GetTransaction(txId, cts.Token);
            var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);

            PrintTransaction(tx, NetworkPrefix, CreateTransactionTable());
        }

        public async Task GetTransactions(uint id, uint start, uint? count)
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            var wallet = new chia.dotnet.Wallet(id, Service);
            var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);

            if (count is null)
            {
                count = await wallet.GetTransactionCount(cts.Token);
            }

            var transactions = await wallet.GetTransactions(start, count.Value - start, cts.Token);
            if (transactions.Any())
            {
                var table = CreateTransactionTable();
                foreach (var tx in transactions)
                {
                    PrintTransaction(tx, NetworkPrefix, table);
                }
                var c = transactions.Count();
                AnsiConsole.Render(table);
                ConsoleMessage.MarkupLine($"[wheat1]{c}[/] transaction{(c == 1 ? string.Empty : "s")}");
            }
            else
            {
                ConsoleMessage.Warning("There are no transactions to this address");
            }
        }
        private static Table CreateTransactionTable()
        {
            var table = new Table();
            table.AddColumn("[orange3]Name[/]");
            table.AddColumn("[orange3]Status[/]");
            table.AddColumn("[orange3]Amount[/]");
            table.AddColumn("[orange3]To[/]");
            table.AddColumn("[orange3]At[/]");
            return table;
        }

        private void PrintTransaction(TransactionRecord tx, string prefix, Table table)
        {
            var name = ConsoleMessage.Verbose ? tx.Name : tx.Name.Substring(2, 10) + "...";
            var status = tx.Confirmed ? "Confirmed"
                : tx.IsInMempool ? "In mempool"
                : "Pending";

            var color = tx.Sent > 0 ? "red" : "green";

            var amount = $"[{color}]{tx.Amount.AsChia()} {prefix}[/]";
            var bech32 = new Bech32M(prefix);
            var to = ConsoleMessage.Verbose ? bech32.PuzzleHashToAddress(tx.ToPuzzleHash) : bech32.PuzzleHashToAddress(tx.ToPuzzleHash).Substring(2, 10) + "...";
            var at = tx.CreatedAtDateTime.ToLocalTime().ToString();

            table.AddRow(name, status, amount, to, at);
        }

        public async Task Send(uint id, string address, decimal amount, decimal fee)
        {
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var (NetworkName, NetworkPrefix) = await Service.GetNetworkInfo(cts.Token);
            var wallet = new chia.dotnet.Wallet(id, Service);
            var tx = await wallet.SendTransaction(address, amount.ToMojo(), fee.ToMojo(), cts.Token);

            PrintTransaction(tx, NetworkPrefix,CreateTransactionTable());

            ConsoleMessage.Helpful($"Do [grey]rchia wallet get-transaction -tx {tx.TransactionId}[/] to get status");
        }
    }
}
