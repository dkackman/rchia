using System;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.PlotNft;

internal sealed class InspectNftCommand : WalletCommand
{
    [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
    public int Id { get; init; } = 1;

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving nft plot info...", async output =>
        {
            if (Id < 0)
            {
                throw new ArgumentException($"{nameof(Id)} cannot be negative.", nameof(Id));
            }

            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var wallet = new PoolWallet((uint)Id, await Login(rpcClient, output));

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            await wallet.Validate(cts.Token);

            var result = await wallet.Status(cts.Token);

            if (Json)
            {
                output.WriteOutput(result);
            }
            else
            {
                output.WriteOutput("launch_id", result.State.LauncherId, Verbose);

                foreach (var tx in result.UnconfirmedTransactions)
                {
                    PrintTransactionSentTo(output, tx);
                }
            }
        });
    }
}
