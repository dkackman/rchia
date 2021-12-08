using rchia.Commands;

namespace rchia.Wallet;

[Command("wallets", Description = "Manage your wallet.\nRequires a wallet or daemon endpoint.")]
internal sealed class WalletsCommands
{
    [Command("show", Description = "Show wallet information")]
    public ShowWalletCommand Show { get; init; } = new();

    [Command("delete-unconfirmed-transactions", Description = "Deletes all unconfirmed transactions from a wallet")]
    public DeleteUnconfirmedTransactionsCommand DeleteUnconfirmedTransactions { get; init; } = new();

    [Command("get-address", Description = "Get a wallet receive address")]
    public GetAddressCommand GetAddress { get; init; } = new();

    [Command("get-transaction", Description = "Show a transaction")]
    public GetTransactionCommand GetTransaction { get; init; } = new();

    [Command("list-transactions", Description = "List all transactions")]
    public GetTransactionsCommand GetTransactions { get; init; } = new();

    [Command("send", Description = "Send chia to another wallet")]
    public SendTransactionCommand Send { get; init; } = new();
}
