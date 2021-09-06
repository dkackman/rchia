﻿using System;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Wallet
{
    internal sealed class GetAddressCommand : SharedOptions
    {
        [Option("fp", "fingerprint", Description = "Set the fingerprint to specify which wallet to use")]
        public uint Fingerprint { get; set; }

        [Option("i", "id", Default = 1, Description = "Id of the wallet to use")]
        public uint Id { get; set; } = 1;

        [Option("n", "new", Default = false, Description = "Flag indicating whether to create a new address")]
        public bool New { get; set; }

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
                    await tasks.GetAddress(Fingerprint, New);
                }
                else
                {
                    await tasks.GetAddress(Id, New);
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