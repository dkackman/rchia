﻿using System;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Keys
{
    internal sealed class DeleteAllKeys : SharedOptions
    {
        [Option("f", "force", Default = false, Description = "Delete all keys without prompting for confirmation")]
        public bool Force { get; set; }

        [CommandTarget]
        public override async Task<int> Run()
        {
            try
            {
                if (Confirm("Deleting all of your keys CANNOT be undone.", "Are you sure you want to delete all of your keys?", Force))
                {
                    using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Wallet);
                    var wallet = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                    var tasks = new KeysTasks(wallet, this);

                    Message("Deleting all keys...");
                    await tasks.DeleteAll();
                    Message("All keys deleted.");
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