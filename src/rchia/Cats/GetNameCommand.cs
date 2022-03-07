using System;
using System.Threading;
using System.Threading.Tasks;
using rchia.Commands;
using chia.dotnet;

namespace rchia.Cats;

internal sealed class GetNameCommand : WalletCommand
{
    [Option("i", "id", Default = 1, Description = "Id of the CAT wallet to use")]
    public int Id { get; init; } = 1;

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving CAT wallet name...", async output =>
        {
            if (Id < 0)
            {
                throw new ArgumentException($"{nameof(Id)} cannot be negative.", nameof(Id));
            }

            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var wallet = new CATWallet((uint)Id, await Login(rpcClient, output));
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            var name = await wallet.GetName(cts.Token);
            output.WriteOutput("Name", name, Verbose);
        });
    }
}
