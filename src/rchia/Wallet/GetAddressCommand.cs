using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Wallet
{
    internal sealed class GetAddressCommand : WalletCommand
    {
        [Option("n", "new", Default = false, Description = "Flag indicating whether to create a new address")]
        public bool New { get; init; }

        [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
        public uint Id { get; init; } = 1;

        [CommandTarget]
        public async Task<int> Run()
        {
            return await DoWorkAsync("Retrieving wallet address...", async ctx =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Wallet);
                var wallet = new chia.dotnet.Wallet(Id, await Login(rpcClient, ctx));

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var address = await wallet.GetNextAddress(New, cts.Token);

                MarkupLine($"[wheat1]{address}[/]");
            });
        }
    }
}
