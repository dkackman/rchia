using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

            var syncStatus = Synced ?
                new Formattable<string>("Synced", "green") :
                Syncing ?
                new Formattable<string>("Syncing", "orange3") :
                new Formattable<string>("Not synced", "red");

            var wallet = new Dictionary<string, object?>()
            {
                // the successful call to Login above means we have a fingerprint for the wallet
                { "network", NetworkName },
                { "prefix", NetworkPrefix },
                { "fingerprint", new Formattable<uint>(proxy.Fingerprint!.Value, fp => $"{fp}") },
                { "sync_status", syncStatus },
                { "wallet_height", height },
            };
            var balances = new List<IDictionary<string, object?>>();

            if (wallets.Any())
            {
                using var status = new StatusMessage(output.Status, "Retrieving balances...");

                foreach (var summary in wallets)
                {
                    var newWallet = new chia.dotnet.Wallet(summary.Id, proxy);
                    var balance = await newWallet.GetBalance(cts.Token);

                    var row = new Dictionary<string, object?>
                    {
                        { "Id", summary.Id },
                        { "Name", summary.Name },
                        { "Type", summary.Type.ToString() },
                        { "Total", balance.ConfirmedWalletBalance.ToChia() },
                        { "Pending Total", balance.UnconfirmedWalletBalance.ToChia() },
                        { "Spendable", balance.SpendableBalance.ToChia() }
                    };

                    if (Verbose || Json)
                    {
                        row.Add("Pending Change", balance.PendingChange.ToChia());
                        row.Add("Max Spend Amount", balance.MaxSendAmount.ToChia());
                        row.Add("Unspent Coin Count", balance.UnspentCoinCount);
                        row.Add("Pending Coin Removal Count", balance.PendingCoinRemovalCount);
                    }

                    balances.Add(row);
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
                result.wallets = balances;

                output.WriteOutput(result);
            }
            else
            {
                output.WriteOutput(wallet);
                output.WriteOutput(balances);
            }
        });
    }
}
