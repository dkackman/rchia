using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Wallet;

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

            var result = new Dictionary<string, object>()
            {
                { "sync_status", Synced ? "Synced" : "Not synced" },
                { "wallet_height", height },
                { "fingerprint", proxy.Fingerprint?.ToString() ?? string.Empty }
            };
            output.WriteOutput(result);

            if (wallets.Any())
            {
                using var status = new StatusMessage(output.Status, "Retrieving balances...");
                var table = new List<IDictionary<string, string>>();

                foreach (var summary in wallets)
                {
                    var newWallet = new chia.dotnet.Wallet(summary.Id, proxy);
                    var (ConfirmedWalletBalance, UnconfirmedWalletBalance, SpendableBalance, PendingChange, MaxSendAmount, UnspentCoinCount, PendingCoinRemovalCount) = await newWallet.GetBalance(cts.Token);

                    var row = new Dictionary<string, string>();
                    row.Add("Id", summary.Id.ToString());
                    row.Add("Name", summary.Name);
                    row.Add("Type", summary.Type.ToString());
                    row.Add("Total", ConfirmedWalletBalance.AsChia());
                    row.Add("Pending Total", UnconfirmedWalletBalance.AsChia());
                    row.Add("Spendable", SpendableBalance.AsChia());

                    if (Verbose || Json)
                    {
                        row.Add("Pending Change", PendingChange.AsChia());
                        row.Add("Max Spend Amount", MaxSendAmount.AsChia());
                        row.Add("Unspent Coin Count", UnspentCoinCount.ToString());
                        row.Add("Pending Coin Removal Count", PendingCoinRemovalCount.ToString());
                    }

                    table.Add(row);
                }

                output.WriteOutput(table);
            }
            else
            {
                output.Warning($"There are no wallets for a this public key {proxy.Fingerprint}");
            }
        });
    }
}
