﻿using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.PlotNft
{
    internal sealed class LeavePoolCommand : WalletCommand
    {
        [Option("i", "id", Default = 1, Description = "Id of the user wallet to use")]
        public uint Id { get; set; } = 1;

        [Option("f", "force", Default = false, Description = "Do not prompt before nft creation")]
        public bool Force { get; set; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                if (Confirm($"Are you sure you want to start self-farming with Plot NFT on wallet id {Id}", Force))
                {
                    using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Wallet);
                    var wallet = await LoginToWallet(rpcClient);
                    var tasks = new PlotNftTasks(wallet, this);

                    await tasks.LeavePool(Id);
                }
            });
        }
    }
}
