using System;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Wallet;

internal sealed class DeleteUnconfirmedTransactionsCommand : WalletCommand
{
    [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
    public int Id { get; init; } = 1;

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Deleting unconfirmed transactions...", async output =>
        {
            if (Id < 0)
            {
                throw new ArgumentException($"{nameof(Id)} cannot be negative.", nameof(Id));
            }

            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var wallet = new chia.dotnet.Wallet((uint)Id, await Login(rpcClient, output));

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            await wallet.DeleteUnconfirmedTransactions(cts.Token);

            output.WriteMarkupLine($"Successfully deleted all unconfirmed transactions for wallet id:");
            output.WriteOutput("wallet", Id, Verbose);
        });
    }
}
