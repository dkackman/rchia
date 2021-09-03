﻿using System;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Keys
{
    internal sealed class ShowKeysCommand : SharedOptions
    {
        [Option("m", "show-mnemonic-seed", Default = false, Description = "Show the mnemonic seed of the keys")]
        public bool ShowMnemonicSeed { get; set; }

        [CommandTarget]
        public override async Task<int> Run()
        {
            try
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Wallet);
                var wallet = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new KeysTasks(wallet, this);

                await tasks.Show(ShowMnemonicSeed);

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