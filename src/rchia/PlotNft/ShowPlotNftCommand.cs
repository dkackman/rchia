﻿using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.PlotNft
{
    internal sealed class ShowPlotNftCommand : WalletCommand
    {
        [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
        public uint Id { get; init; } = 1;

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Wallet);
                var wallet = await LoginToWallet(rpcClient);
                var tasks = new PlotNftTasks(wallet, this);

                await tasks.Show(Id);
            });
        }
    }
}
