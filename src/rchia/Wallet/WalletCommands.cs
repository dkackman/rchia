﻿using rchia.Commands;

namespace rchia.Wallet
{
    [Command("wallet", Description = "Manage your wallet.\nRequires a wallet or daemon endpoint.")]
    internal sealed class WalletCommands
    {
        [Command("list", Description = "Lists all the wallets")]
        public ListWalletsCommand List { get; set; } = new();

        [Command("show", Description = "Show wallet information")]
        public ShowWalletCommand Show { get; set; } = new();

        [Command("delete-unconfirmed-transactions", Description = "Deletes all unconfirmed transactions for this wallet ID")]
        public DeleteUnconfirmedTransactionsCommand DeleteUnconfirmedTransactions { get; set; } = new();

        [Command("get-address", Description = "Get a wallet receive address")]
        public GetAddressCommand GetAddress { get; set; } = new();

        [Command("get-transaction", Description = "Get a transaction")]
        public GetTransactionCommand GetTransaction { get; set; } = new();

        [Command("get-transactions", Description = "Get all transactions")]
        public GetTransactionsCommand GetTransactions { get; set; } = new();

        [Command("send", Description = "Send chia to another wallet")]
        public SendTransactionCommand Send { get; set; } = new();
    }
}