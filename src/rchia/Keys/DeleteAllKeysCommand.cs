using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Keys
{
    internal sealed class DeleteAllKeys : EndpointOptions
    {
        [Option("f", "force", Default = false, Description = "Delete all keys without prompting for confirmation")]
        public bool Force { get; init; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var tasks = await CreateTasks<KeysTasks, WalletProxy>(ServiceNames.Wallet);

                if (Confirm($"Deleting all of your keys [wheat1]CANNOT[/] be undone.\nAre you sure you want to delete all keys from [red]{tasks.Service.RpcClient.Endpoint.Uri}[/]?", Force))
                {
                    await DoWork("Deleting all keys...", async ctx => { await tasks.DeleteAll(); });
                }
            });
        }
    }
}
