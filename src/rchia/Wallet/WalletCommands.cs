using rchia.Commands;

namespace rchia.Wallet
{
    [Command("wallet", Description = "Manage your wallet.\nRequires a wallet or daemon endpoint.")]
    internal sealed class WalletCommands
    {
        [Command("list", Description = "Lists all the wallets")]
        public ListWalletsCommand List { get; set; } = new();

        [Command("show", Description = "Show wallet information")]
        public ShowWalletCommand Show { get; set; } = new();
    }
}


/*
 *   delete_unconfirmed_transactions
                                  Deletes all unconfirmed transactions for
                                  this wallet ID

  get_address                     Get a wallet receive address
  get_transaction                 Get a transaction
  get_transactions                Get all transactions
  send                            Send chia to another wallet
  show                            Show wallet information
*/
