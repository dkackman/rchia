using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Dynamic;
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

            var wallet = new Dictionary<string, object?>()
            {
                { "fingerprint", proxy.Fingerprint?.ToString() ?? string.Empty },
                { "sync_status", Synced ? "Synced" : "Not synced" },
                { "wallet_height", height }
            };
            var table = new List<IDictionary<string, object?>>();

            if (wallets.Any())
            {
                using var status = new StatusMessage(output.Status, "Retrieving balances...");

                foreach (var summary in wallets)
                {
                    var newWallet = new chia.dotnet.Wallet(summary.Id, proxy);
                    var (ConfirmedWalletBalance, UnconfirmedWalletBalance, SpendableBalance, PendingChange, MaxSendAmount, UnspentCoinCount, PendingCoinRemovalCount) = await newWallet.GetBalance(cts.Token);

                    var row = new Dictionary<string, object?>();
                    row.Add("Id", summary.Id);
                    row.Add("Name", summary.Name);
                    row.Add("Type", summary.Type.ToString());
                    row.Add("Total", ConfirmedWalletBalance.ToChia());
                    row.Add("Pending Total", UnconfirmedWalletBalance.ToChia());
                    row.Add("Spendable", SpendableBalance.ToChia());

                    if (Verbose || Json)
                    {
                        row.Add("Pending Change", PendingChange.ToChia());
                        row.Add("Max Spend Amount", MaxSendAmount.ToChia());
                        row.Add("Unspent Coin Count", UnspentCoinCount);
                        row.Add("Pending Coin Removal Count", PendingCoinRemovalCount);
                    }

                    table.Add(row);
                }
            }
            else
            {
                output.WriteWarning($"There are no wallets for a this public key {proxy.Fingerprint}");
            }

            if (Json)
            {
                dynamic result = new ExpandoObject();
                result.summary = wallet;
                result.wallets = table;

                output.WriteOutput(result);
            }
            else
            {
                output.WriteOutput(wallet);
                output.WriteOutput(table);
            }
        });
    }
}
