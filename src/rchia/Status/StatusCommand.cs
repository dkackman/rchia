using rchia.Commands;

namespace rchia.Status
{
    [Command("status", Description = "Shows the status of the node.\nRequires a daemon endpoint.")]
    internal sealed class StatusCommand
    {
        [Command("services", Description = "Show which services are running on the node")]
        public ServicesCommand Services { get; init; } = new();

        [Command("ping", Description = "Pings the daemon")]
        public PingCommand Ping { get; init; } = new();
    }
}
