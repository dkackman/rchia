using System;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Wallet
{
    internal sealed class SendTransactionCommand : WalletCommand
    {
        [Option("a", "amount", IsRequired = true, Description = "How much chia to send, in XCH")]
        public decimal Amount { get; init; }

        [Option("m", "fee", Default = 0, Description = "Set the fees for the transaction, in XCH")]
        public decimal Fee { get; init; }

        [Option("t", "address", IsRequired = true, Description = "Address to send the XCH")]
        public string Address { get; init; } = string.Empty;

        [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
        public uint Id { get; init; } = 1;

        [Option("f", "force", Default = false, Description = "If Fee > Amount, send the transaction anyway")]
        public bool Force { get; init; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await DoWorkAsync("Sending Transaction...", async ctx =>
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
                    Warning($"A transaction of amount {Amount} and fee {Fee} is unusual.");
                    throw new InvalidOperationException("Pass in --force if you are sure you mean to do this.");
                }

                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, this, ServiceNames.Wallet);
                var wallet = new chia.dotnet.Wallet(Id, await Login(rpcClient));

                using var cts = new CancellationTokenSource(TimeoutMilliseconds);
                var (NetworkName, NetworkPrefix) = await wallet.WalletProxy.GetNetworkInfo(cts.Token);
                var tx = await wallet.SendTransaction(Address, Amount.ToMojo(), Fee.ToMojo(), cts.Token);

                PrintTransaction(tx, NetworkPrefix, CreateTransactionTable());

                Helpful($"Do '[grey]rchia wallet get-transaction -tx {tx.TransactionId}[/]' to get status");
            });
        }
    }
}
