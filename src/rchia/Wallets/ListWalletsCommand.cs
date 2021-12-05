using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Wallet
{
    internal sealed class ListWalletsCommand : EndpointOptions
    {
        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Retrieving wallet list...", async output =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
                var proxy = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var keys = await proxy.GetPublicKeys(cts.Token);
                if (!keys.Any())
                {
                    throw new InvalidOperationException("No public keys found. You'll need to run 'rchia keys generate'");
                }

                var result = new Dictionary<string, IEnumerable<IDictionary<string, string>>>();
                foreach (var fingerprint in keys)
                {
                    using var cts1 = new CancellationTokenSource(TimeoutMilliseconds);
                    _ = proxy.LogIn(fingerprint, false, cts1.Token);
                    var wallets = await proxy.GetWallets(cts1.Token);

                    var table = new List<IDictionary<string, string>>();
                    foreach (var wallet in wallets)
                    {
                        var row = new Dictionary<string, string>
                        {
                            { "Id", wallet.Id.ToString() },
                            { "Name", wallet.Name },
                            { "Type", wallet.Type.ToString() }
                        };
                    }

                    result.Add(fingerprint.ToString(), table);
                }
                output.WriteOutput(output);
            });
        }
    }
}
