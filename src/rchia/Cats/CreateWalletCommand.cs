using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using rchia.Commands;
using chia.dotnet;

namespace rchia.Cats;

internal sealed class CreateWalletCommand : WalletCommand
{
    [Option("a", "amount", IsRequired = true, Description = "The amount to deposit in the wallet, in XCH")]
    public decimal Amount { get; init; }

    [Option("m", "fee", Default = 0, Description = "Set the fees for the wallet creation transaction, in XCH")]
    public decimal Fee { get; init; }

    [Option("n", "name", Default = null, Description = "The name of the new wallet")]
    public string? Name { get; init; }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Creating new CAT wallet...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var proxy = await Login(rpcClient, output);
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            var wallet = !string.IsNullOrEmpty(Name)
                ? await proxy.CreateCATWallet(Name, Amount.ToMojo(), Fee.ToMojo(), cts.Token)
                : await proxy.CreateCATWallet(Amount.ToMojo(), Fee.ToMojo(), cts.Token);

            if (Json)
            {
                output.WriteOutput(wallet);
            }
            else
            {
                var result = new Dictionary<string, object?>()
                {
                    { "Type", wallet.Type },
                    { "Asset Id", wallet.AssetId },
                    { "Wallet Id", wallet.WalletId },
                };

                output.WriteOutput(result);
            }
        });
    }
}
