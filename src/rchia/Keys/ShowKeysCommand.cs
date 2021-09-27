using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Bech32;
using rchia.Commands;

namespace rchia.Keys
{
    internal sealed class ShowKeysCommand : EndpointOptions
    {
        [Option("m", "show-mnemonic-seed", Default = false, Description = "Show the mnemonic seed of the keys")]
        public bool ShowMnemonicSeed { get; init; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await DoWorkAsync("Retrieving kets...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Wallet);

                var proxy = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var keys = await proxy.GetPublicKeys(cts.Token);

                if (keys.Any())
                {
                    var (NetworkName, NetworkPrefix) = await proxy.GetNetworkInfo(cts.Token);
                    var bech32 = new Bech32M(NetworkPrefix);

                    foreach (var fingerprint in keys)
                    {
                        NameValue("Fingerprint", fingerprint);
                        using var cts1 = new CancellationTokenSource(TimeoutMilliseconds);
                        var (Fingerprint, Sk, Pk, FarmerPk, PoolPk, Seed) = await proxy.GetPrivateKey(fingerprint, cts1.Token);

                        NameValue("Master public key (m)", Pk);
                        NameValue("Farmer public key(m/12381/8444/0/0)", FarmerPk);
                        NameValue("Pool public key (m/12381/8444/1/0)", PoolPk);

                        // this isn't possible over rpc right now
                        //var address = bech32.PuzzleHashToAddress(HexBytes.FromHex(Sk));
                        // Console.WriteLine($"First wallet address", address(;

                        if (ShowMnemonicSeed)
                        {
                            NameValue("Master private key (m)", Sk);

                            // this isn't possible over rpc right now
                            //Console.WriteLine($"First wallet secret key (m/12381/8444/2/0)", Sk(;

                            NameValue("  Mnemonic seed (24 secret words)", Seed);
                        }
                    }
                }
                else
                {
                    Warning("There are no saved private keys");
                }
            });
        }
    }
}
