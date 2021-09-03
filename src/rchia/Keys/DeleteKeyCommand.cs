using System;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Keys
{
    internal sealed class DeleteKeyCommand : SharedOptions
    {
        [Option("fp", "fingerprint", IsRequired = true, ArgumentHelpName = "FINGERPRINT", Description = "Enter the fingerprint of the key you want to delete")]
        public uint Fingerprint { get; set; }

        [Option("f", "force", Default = false, Description = "Delete the key without prompting for confirmation")]
        public bool Force { get; set; }

        [CommandTarget]
        public override async Task<int> Run()
        {
            try
            {
                if (Fingerprint == 0)
                {
                    throw new InvalidOperationException($"{Fingerprint} is not a valid wallet fingerprint");
                }

                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Wallet);
                var wallet = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                var commands = new KeysTasks(wallet, this);

                if (Confirm("Deleting a key CANNOT be undone.", $"Are you sure you want to delete this key {Fingerprint}?", Force))
                {
                    Message($"Deleting key {Fingerprint}...");
                    await commands.Delete(Fingerprint);
                }

                return 0;
            }
            catch (Exception e)
            {
                Message(e);

                return -1;
            }
        }
    }
}
