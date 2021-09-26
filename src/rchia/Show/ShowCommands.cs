using rchia.Commands;

namespace rchia.Show
{
    [Command("show", Description = "Shows various properties of a full node.\nRequires a daemon or full_node endpoint.")]
    internal sealed class ShowCommands
    {
        [Command("add", Description = "Connect to another Full Node by ip:port")]
        public AddConnectionCommand Add { get; init; } = new();

        [Command("exit", Description = "Shut down the running Full Node")]
        public ExitNodeCommand Exit { get; init; } = new();

        [Command("header", Description = "Look up a block header hash by block height or hash")]
        public BlockHeaderCommand BlockHeader { get; init; } = new();

        [Command("state", Description = "Show the current state of the blockchain")]
        public StateCommand State { get; init; } = new();

        [Command("connections", Description = "List nodes connected to this Full Node")]
        public ConnectionsCommand Connections { get; init; } = new();

        [Command("prune", Description = "Prune stale connections")]
        public PruneCommand Prune { get; init; } = new();

        [Command("remove", Description = "Remove a Node by the full or first 8 characters of NodeID")]
        public RemoveConnectionCommand Remove { get; init; } = new();
    }
}
