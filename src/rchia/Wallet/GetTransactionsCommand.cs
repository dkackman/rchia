﻿using System;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Wallet
{
    internal sealed class GetTransactionsCommand : SharedOptions
    {
        [Option("fp", "fingerprint", Description = "Set the fingerprint to specify which wallet to use")]
        public uint Fingerprint { get; set; }

        [Option("i", "id", Default = 1, Description = "Id of the wallet to use")]
        public uint Id { get; set; } = 1;

        [CommandTarget]
        public override async Task<int> Run()
        {
            try
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, ServiceNames.Wallet);
                var wallet = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new WalletTasks(wallet, this);

                if (Fingerprint > 0)
                {
                    var id = await wallet.GetWalletId(Fingerprint);
                    await tasks.GetTransactions(id);
                }
                else
                {
                    await tasks.GetTransactions(Id);
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