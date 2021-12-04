using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using Spectre.Console;

namespace rchia.Wallet
{
    internal sealed class ShowWalletCommand : WalletCommand
    {
        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Retrieving wallet info...", async output =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
                var proxy = await Login(rpcClient, output);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var (GenesisInitialized, Synced, Syncing) = await proxy.GetSyncStatus(cts.Token);
                var (NetworkName, NetworkPrefix) = await proxy.GetNetworkInfo(cts.Token);
                var height = await proxy.GetHeightInfo(cts.Token);
                var wallets = await proxy.GetWallets(cts.Token);

                NameValue("Sync status", Synced ? "[green]Synced[/]" : "[red]Not synced[/]");
                NameValue("Wallet height", height);
                NameValue("Fingerprint", proxy.Fingerprint);

                if (wallets.Any())
                {
                    using var status = new StatusMessage(output.Status, "Retrieving balances...");
                    var table = new Table
                    {
                        Title = new TableTitle($"[orange3]Balances ({NetworkPrefix})[/]")
                    };

                    table.AddColumn("[orange3]Id[/]");
                    table.AddColumn("[orange3]Name[/]");
                    table.AddColumn("[orange3]Type[/]");
                    table.AddColumn("[orange3]Total[/]");
                    table.AddColumn("[orange3]Pending Total[/]");
                    table.AddColumn("[orange3]Spendable[/]");
                    if (Verbose)
                    {
                        table.AddColumn("[orange3]Pending Change[/]");
                        table.AddColumn("[orange3]Max Spend Amount[/]");
                        table.AddColumn("[orange3]Unspent Coin Count[/]");
                        table.AddColumn("[orange3]Pending Coin Removal Count[/]");
                    }

                    foreach (var summary in wallets)
                    {
                        var newWallet = new chia.dotnet.Wallet(summary.Id, proxy);
                        var (ConfirmedWalletBalance, UnconfirmedWalletBalance, SpendableBalance, PendingChange, MaxSendAmount, UnspentCoinCount, PendingCoinRemovalCount) = await newWallet.GetBalance(cts.Token);

                        if (Verbose)
                        {
                            table.AddRow(summary.Id.ToString(), summary.Name, $"[green]{summary.Type}[/]", ConfirmedWalletBalance.AsChia(), UnconfirmedWalletBalance.AsChia(), SpendableBalance.AsChia(),
                                PendingChange.AsChia(), MaxSendAmount.AsChia(), UnspentCoinCount.ToString(), PendingCoinRemovalCount.ToString());
                        }
                        else
                        {
                            table.AddRow(summary.Id.ToString(), summary.Name, $"[green]{summary.Type}[/]", ConfirmedWalletBalance.AsChia(), UnconfirmedWalletBalance.AsChia(), SpendableBalance.AsChia());
                        }
                    }

                    AnsiConsole.Write(table);
                }
                else
                {
                    Warning($"There are no wallets for a this public key {proxy.Fingerprint}");
                }
            });
        }
    }
}
