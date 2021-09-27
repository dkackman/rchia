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
        public async override Task<int> Run()
        {
            return await DoWorkAsync("Retrieving wallet info...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Wallet);
                var proxy = await Login(rpcClient);

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
                    MarkupLine($"[orange3]Balances ({NetworkPrefix})[/]");
                    var table = new Table();
                    table.AddColumn("[orange3]Id[/]");
                    table.AddColumn("[orange3]Type[/]");
                    table.AddColumn("[orange3]Name[/]");
                    table.AddColumn("[orange3]Total[/]");
                    table.AddColumn("[orange3]Pending Total[/]");
                    table.AddColumn("[orange3]Spendable[/]");

                    foreach (var summary in wallets)
                    {
                        var newWallet = new chia.dotnet.Wallet(summary.Id, proxy);
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
                else
                {
                    Warning($"There are no wallets for a this public key {proxy.Fingerprint}");
                }
            });
        }
    }
}
