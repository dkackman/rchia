﻿using System;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;

namespace rchia.Show
{
    [Command("show", Description = "Shows various properties of a full node.\nRequires a daemon or full_node endpoint.")]
    internal sealed class ShowCommand : EndpointOptions
    {
        [Option("a", "add-connection", ArgumentHelpName = "URI", Description = "Connect to another Full Node by ip:port")]
        public string? AddConnection { get; init; }

        [Option("b", "block-by-header-hash", ArgumentHelpName = "HASH", Description = "Look up a block by block header hash")]
        public string? BlockByHeaderHash { get; init; }

        [Option("bh", "block-header-hash-by-height", ArgumentHelpName = "HEIGHT", Description = "Look up a block header hash by block height")]
        public uint? BlockHeaderHashByHeight { get; init; }

        [Option("c", "connections", Description = "List nodes connected to this Full Node")]
        public bool Connections { get; init; }

        [Option("e", "exit-node", Description = "Shut down the running Full Node")]
        public bool Exit { get; init; }

        [Option("r", "remove-connection", ArgumentHelpName = "NODE ID", Description = "Remove a Node by the full or first 8 characters of NodeID")]
        public string? RemoveConnection { get; init; }

        [Option("s", "state", Description = "Show the current state of the blockchain")]
        public bool State { get; init; }

        [Command("prune", Description = "Prune stale connections")]
        public PruneCommand Prune { get; init; } = new();

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, ServiceNames.FullNode);
                var fullNode = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new ShowTasks(fullNode, this);

                if (State)
                {
                    await DoWork("Retrieving node info...", async ctx => { await tasks.State(); });

                }
                else if (Exit)
                {
                    await DoWork("Shutting down the node...", async ctx => { await tasks.Exit(); });

                }
                else if (Connections)
                {
                    await DoWork("Retrieving connections...", async ctx => { await tasks.Connections(); });

                }
                else if (!string.IsNullOrEmpty(AddConnection))
                {
                    await DoWork("Adding connection...", async ctx => { await tasks.AddConnection(AddConnection); });

                }
                else if (!string.IsNullOrEmpty(RemoveConnection))
                {
                    await DoWork("Removing connections...", async ctx => { await tasks.RemoveConnection(RemoveConnection); });

                }
                else if (!string.IsNullOrEmpty(BlockByHeaderHash))
                {
                    await DoWork("Retrieving block header...", async ctx => { await tasks.BlockByHeaderHash(BlockByHeaderHash); });

                }
                else if (BlockHeaderHashByHeight.HasValue)
                {
                    await DoWork("Retrieving block header...", async ctx => { await tasks.BlockHeaderHashByHeight(BlockHeaderHashByHeight.Value); });
                }
                else
                {
                    throw new InvalidOperationException("Unrecognized command");
                }
            });
        }
    }
}
