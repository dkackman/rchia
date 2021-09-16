﻿using System;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Keys
{
    internal sealed class DeleteKeyCommand : EndpointOptions
    {
        [Option("fp", "fingerprint", IsRequired = true, ArgumentHelpName = "FINGERPRINT", Description = "Enter the fingerprint of the key you want to delete")]
        public uint Fingerprint { get; set; }

        [Option("f", "force", Default = false, Description = "Delete the key without prompting for confirmation")]
        public bool Force { get; set; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                if (Fingerprint == 0)
                {
                    throw new InvalidOperationException($"{Fingerprint} is not a valid wallet fingerprint");
                }

                if (Confirm($"Deleting a key CANNOT be undone.\nAre you sure you want to delete key {Fingerprint}?", Force))
                {
                    using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Wallet);
                    var wallet = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);
                    var commands = new KeysTasks(wallet, this);

                    Message($"Deleting key {Fingerprint}...");
                    await commands.Delete(Fingerprint);
                }
            });
        }
    }
}
