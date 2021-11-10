using rchia.Commands;

namespace rchia.Node
{
    [Command("node", Description = "Commands to managing the node.\nRequires a daemon endpoint.")]
    internal sealed class NodeCommands
    {
        [Command("ping", Description = "Pings the daemon")]
        public PingCommand Ping { get; init; } = new();

        [Command("version", Description = "Shows the version of the node")]
        public VersionCommand Version { get; init; } = new();

        [Command("stop", Description = "Stops the node")]
        public StopNodeCommand Remove { get; init; } = new();

        [Command("status", Description = "Show the current status of the blockchain")]
        public StatusCommand Status { get; init; } = new();
    }
}
