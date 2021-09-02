using System;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Keys
{
    internal sealed class DeleteKeyCommand : SharedOptions
    {

        [Argument(0, Name = "fingerprint", Description = "Enter the fingerprint of the key you want to use")]
        public uint Fingerprint { get; set; }

        [Option("f", "force", Description = "Do not prompt before deleting keys")]
        public bool Force { get; set; }

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
