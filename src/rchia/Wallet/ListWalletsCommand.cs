using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using Spectre.Console;

namespace rchia.Wallet
{
    internal sealed class ListWalletsCommand : EndpointOptions
    {
        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Retrieving wallet list...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Wallet);
                var proxy = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var keys = await proxy.GetPublicKeys(cts.Token);
                if (!keys.Any())
                {
                    throw new InvalidOperationException("No public keys found. You'll need to run 'rchia keys generate'");
                }

                foreach (var fingerprint in keys)
                {
                    NameValue("Fingerprint", fingerprint);

                    using var cts1 = new CancellationTokenSource(TimeoutMilliseconds);
                    _ = proxy.LogIn(fingerprint, false, cts1.Token);
                    var wallets = await proxy.GetWallets(cts1.Token);

                    var table = new Table();
                    table.AddColumn("[orange3]Id[/]");
                    table.AddColumn("[orange3]Name[/]");
                    table.AddColumn("[orange3]Type[/]");

                    foreach (var wallet in wallets)
                    {
                        table.AddRow(wallet.Id.ToString(), wallet.Name, $"[green]{wallet.Type}[/]");
                    }

                    AnsiConsole.Render(table);
                }
            });
        }
    }
}
