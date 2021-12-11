using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Keys;

internal sealed class ShowKeysCommand : EndpointOptions
{
    [Option("m", "show-mnemonic-seed", Description = "Show the mnemonic seed of the keys")]
    public bool ShowMnemonicSeed { get; init; }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving keys...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var proxy = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var keys = await proxy.GetPublicKeys(cts.Token);

            var table = new List<IDictionary<string, object?>>();
            foreach (var fingerprint in keys)
            {
                using var cts1 = new CancellationTokenSource(TimeoutMilliseconds);
                var (Fingerprint, Sk, Pk, FarmerPk, PoolPk, Seed) = await proxy.GetPrivateKey(fingerprint, cts1.Token);

                var row = new Dictionary<string, object?>
                {
                    { "fingerprint", fingerprint },
                    { "master_public_key", Pk },
                    { "farmer_public_key", FarmerPk },
                    { "pool_public_key", PoolPk }
                };

                if (ShowMnemonicSeed)
                {
                    row.Add("master_private_key", Sk);
                    row.Add("mnemonic_seed", Seed);
                }

                table.Add(row);
            }

            output.WriteOutput(table);
        });
    }
}
