using System;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Wallet;

internal sealed class GetAddressCommand : WalletCommand
{
    [Option("n", "new", Description = "Flag indicating whether to create a new address")]
    public bool New { get; init; }

    [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
    public int Id { get; init; } = 1;

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving wallet address...", async output =>
        {
            if (Id < 0)
            {
                throw new ArgumentException($"{nameof(Id)} cannot be negative.", nameof(Id));
            }

            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var wallet = new chia.dotnet.Wallet((uint)Id, await Login(rpcClient, output));
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            var address = await wallet.GetNextAddress(New, cts.Token);

            output.WriteOutput("address", address, Verbose);
        });
    }
}
