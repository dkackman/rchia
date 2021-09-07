using System;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Wallet
{
    internal sealed class SendTransactionCommand : SharedOptions
    {
        [Option("fp", "fingerprint", Description = "Set the fingerprint to specify which wallet to use")]
        public uint Fingerprint { get; set; }

        [Option("i", "id", Default = 1, Description = "Id of the wallet to use")]
        public uint Id { get; set; } = 1;

        [Option("a", "amount", IsRequired = true, Description = "How much chia to send, in XCH")]
        public decimal Amount { get; set; }

        [Option("m", "fee", Default = 0, Description = "Set the fees for the transaction, in XCH")]
        public decimal Fee { get; set; }

        [Option("t", "address", IsRequired = true, Description = "Address to send the XCH")]
        public string Address { get; set; } = string.Empty;

        [Option("f", "force", Default = false, Description = "If Fee > Amount, send the transaction anyway")]
        public bool Force { get; set; }

        [CommandTarget]
        public override async Task<int> Run()
        {
            return await Execute(async () =>
            {
                if (string.IsNullOrEmpty(Address))
                {
                    throw new InvalidOperationException("Address cannot be empty");
                }

                if (Amount < 0)
                {
                    throw new InvalidOperationException("Amount cannot be negative");
                }

                if (Fee < 0)
                {
                    throw new InvalidOperationException("Fee cannot be negative");
                }

                if (Fee > Amount && !Force)
                {
                    Console.WriteLine($"A transaction of amount {Amount} and fee {Fee} is unusual.");
                    throw new InvalidOperationException("Pass in --force if you are sure you mean to do this.");
                }

                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, ServiceNames.Wallet);
                var wallet = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new WalletTasks(wallet, this);

                if (Fingerprint > 0)
                {
                    var idForFingerprint = await wallet.GetWalletId(Fingerprint);
                    await tasks.Send(idForFingerprint, Address, Amount, Fee);
                }
                else
                {
                    await tasks.Send(Id, Address, Amount, Fee);
                }
            });
        }
    }
}
