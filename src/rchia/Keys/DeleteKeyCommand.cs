using System;
using System.Threading.Tasks;

using rchia.Commands;

namespace rchia.Keys
{
    internal sealed class DeleteKeyCommand : WalletCommand
    {
        [Option("f", "force", Default = false, Description = "Delete the key without prompting for confirmation")]
        public bool Force { get; init; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                if (Fingerprint is null || Fingerprint == 0)
                {
                    throw new InvalidOperationException($"{Fingerprint} is not a valid wallet fingerprint");
                }

                using var tasks = new KeysTasks(await Login(), this, TimeoutMilliseconds);

                if (Confirm($"Deleting a key CANNOT be undone.\nAre you sure you want to delete key {Fingerprint} from [red]{tasks.Service.RpcClient.Endpoint.Uri}[/]?", Force))
                {
                    await DoWork("Deleting key...", async ctx => { await tasks.Delete(Fingerprint.Value); });
                }
            });
        }
    }
}
