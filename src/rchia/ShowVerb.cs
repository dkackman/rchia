using System;
using System.Threading.Tasks;

using chia.dotnet;
using chia.dotnet.console;

using rchia.Show;

using CommandLine;

namespace rchia
{
    [Verb("show", isDefault:true, HelpText = "Shows various properties of a full node")]
    internal sealed class ShowVerb : SharedOptions
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
                using var rpcClient = await Program.Factory.CreateRpcClient(this, ServiceNames.FullNode);
                var fullNode = new FullNodeProxy(rpcClient, Program.Factory.OriginService);

                if (State)
                {
                    await StateTask.Run(fullNode);
                }
                else if (Exit)
                {
                    await ExitTask.Run(fullNode, Verbose);
                }
                else if (Connections)
                {
                    await ConnectionsTask.Run(fullNode, Verbose);
                }
                else if (!string.IsNullOrEmpty(AddConnection))
                {
                    await AddConnectionTask.Run(fullNode, AddConnection, Verbose);
                }
                else if (!string.IsNullOrEmpty(RemoveConnection))
                {
                    await RemoveConnectionTask.Run(fullNode, RemoveConnection, Verbose);
                }
                else if (!string.IsNullOrEmpty(BlockByHeaderHash))
                {
                    await BlockByHeaderHashTask.Run(fullNode, BlockByHeaderHash, Verbose);
                }
                else
                {
                    Console.WriteLine("Unrecognized command");
                    return -1;
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
