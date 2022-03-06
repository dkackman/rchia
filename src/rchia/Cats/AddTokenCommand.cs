using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using rchia.Commands;
using chia.dotnet;

namespace rchia.Cats;

internal sealed class AddTokenCommand : WalletCommand
{
    [Option("a", "asset-id", IsRequired = true, Description = "Id of the asset to add")]
    public string AssetId { get; init; } = string.Empty;

    [Option("n", "name", Description = "The token name")]
    public string Name { get; init; } = string.Empty;

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Adding token...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var proxy = await Login(rpcClient, output);
            var tradeManager = new TradeManager(proxy);
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            var existingInfo = await tradeManager.AssetIdToName(AssetId, cts.Token);

            var walletId = (uint)0;

            var result = new Dictionary<string, object?>
            {
                { "exisitng_wallet", existingInfo.WalletId.HasValue }
            };

            if (existingInfo.WalletId.HasValue)
            {
                walletId = existingInfo.WalletId.Value;
            }
            else
            {
                var newWalletInfo = await proxy.CreateWalletForCAT(AssetId, cts.Token);
                walletId = newWalletInfo.WalletId;
            }

            var wallet = new CATWallet(walletId, proxy);
            await wallet.SetName(Name, cts.Token);

            result.Add("wallet_id", walletId);
            result.Add("asset_id", AssetId);
            result.Add("name", Name);
            result.Add("fingerprint", Fingerprint);
            output.WriteOutput(result);
        });
    }
}
