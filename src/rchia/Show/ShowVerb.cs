using System;
using System.Threading.Tasks;

using chia.dotnet;
using chia.dotnet.console;

using CommandLine;

namespace rchia.Show
{
    [Verb("show", HelpText = "Shows various properties of a full node.\nRequires a daemon or full_node endpoint.")]
    internal sealed class Show : SharedOptions
    {
        [Option('a', "add-connection", HelpText = "[URI] Connect to another Full Node by ip:port")]
        public string? AddConnection { get; set; }

        [Option('b', "block-by-header-hash", HelpText = "[HASH] Look up a block by block header hash")]
        public string? BlockByHeaderHash { get; set; }

        [Option('c', "connections", HelpText = "List nodes connected to this Full Node")]
        public bool Connections { get; set; }

        [Option('e', "exit-node", HelpText = "Shut down the running Full Node")]
        public bool Exit { get; set; }

        [Option('r', "remove-connection", HelpText = "[NODE ID] Remove a Node by the full or first 8 characters of NodeID")]
        public string? RemoveConnection { get; set; }

        [Option('s', "state", HelpText = "Show the current state of the blockchain")]
        public bool State { get; set; }

        public override async Task<int> Run()
        {
            try
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, ServiceNames.FullNode);
                var fullNode = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);
                var commands = new ShowTasks(fullNode, Verbose, this);

                if (State)
                {
                    await commands.State();
                }
                else if (Exit)
                {
                    await commands.Exit();
                }
                else if (Connections)
                {
                    await commands.Connections();
                }
                else if (!string.IsNullOrEmpty(AddConnection))
                {
                    await commands.AddConnection(AddConnection);
                }
                else if (!string.IsNullOrEmpty(RemoveConnection))
                {
                    await commands.RemoveConnection(RemoveConnection);
                }
                else if (!string.IsNullOrEmpty(BlockByHeaderHash))
                {
                    await commands.BlockByHeaderHash(BlockByHeaderHash);
                }
                else
                {
                    throw new InvalidOperationException("Unrecognized command");
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
