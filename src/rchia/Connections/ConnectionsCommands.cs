using rchia.Commands;

namespace rchia.Connections
{
    [Command("connections", Description = "Various methods for managing node connections.\nRequires a daemon or full_node endpoint.")]
    internal sealed class BlocksCommands
    {
        [Command("add", Description = "Connect to another Full Node by ip:port")]
        public AddConnectionCommand Add { get; init; } = new();

        [Command("list", Description = "List nodes connected to this Full Node")]
        public ListConnectionsCommand Connections { get; init; } = new();

        [Command("prune", Description = "Prune stale connections")]
        public PruneConnectionsCommand Prune { get; init; } = new();

        [Command("remove", Description = "Remove a Node by the full or first 8 characters of NodeID")]
        public RemoveConnectionCommand Remove { get; init; } = new();
    }
}
