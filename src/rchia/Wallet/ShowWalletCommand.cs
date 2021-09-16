﻿using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Wallet
{
    internal sealed class ShowWalletCommand : WalletCommand
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, ServiceNames.Wallet);
                var wallet = await LoginToWallet(rpcClient);
                var tasks = new WalletTasks(wallet, this);

                await DoWork("Retrieving wallet info...", async ctx => { await tasks.Show(); });
            });
        }
    }
}
