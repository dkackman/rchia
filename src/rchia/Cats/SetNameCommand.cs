using System.Threading;
using System.Threading.Tasks;
using rchia.Commands;
using chia.dotnet;

namespace rchia.Cats;

internal sealed class SetNameCommand : WalletCommand
{
    [Option("i", "id", Default = 1, Description = "Id of the CAT wallet to use")]
    public int Id { get; init; } = 1;

    [Option("n", "name", IsRequired = true, Description = "The new name of the wallet")]
    public string Name { get; init; } = string.Empty;

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Setting wallet name...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var wallet = new CATWallet((uint)Id, await Login(rpcClient, output));
            using var cts = new CancellationTokenSource(TimeoutMilliseconds);

            await wallet.SetName(Name, cts.Token);
            output.WriteOutput("status", "success", Verbose);
        });
    }
}
