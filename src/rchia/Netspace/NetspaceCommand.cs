﻿using System;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Netspace
{
    [Command("netspace", Description = "Shows various properties of a full node.\nRequires a daemon or full_node endpoint.")]
    internal sealed class NetspaceCommand : SharedOptions
    {
        [Option("d", "delta-block-height", ArgumentHelpName = "DELTA", Description = "Compare a block X blocks older to estimate total network space.Defaults to 4608 blocks\n" +
                                                                                         "(~1 day) and Peak block as the starting block.Use --start BLOCK_HEIGHT to specify\n" +
                                                                                         "starting block.Use 192 blocks to estimate over the last hour.")]
        public uint DeltaBlockHeight { get; set; } = 4608;

        [Option("s", "start", ArgumentHelpName = "HEADER_HASH", Description = "Newest block used to calculate estimated total network space.Defaults to Peak block.")]
        public string? Start { get; set; }

        [CommandTarget]
        public override async Task<int> Run()
        {
            try
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.FullNode);
                var fullNode = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new NetspaceTasks(fullNode, this);

                await tasks.Netspace(Start, DeltaBlockHeight);
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