using System.Threading;
using System.Threading.Tasks;
using rchia.Commands;
using chia.dotnet;

namespace rchia.Cats;

internal sealed class GetNameCommand : WalletCommand
{
    [Option("i", "id", Default = 1, Description = "Id of the CAT wallet to use")]
    public uint Id { get; init; } = 1;

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving CAT wallet name...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var wallet = new CATWallet(Id, await Login(rpcClient, output));
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            var name = await wallet.GetName(cts.Token);
            output.WriteOutput("Name", name, Verbose);
        });
    }
}
