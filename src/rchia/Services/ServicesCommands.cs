using rchia.Commands;

namespace rchia.Services;

[Command("services", Description = "Shows the status of the node.\nRequires a daemon endpoint.")]
internal sealed class StatusCommand
{
    [Command("list", Description = "Show which services are running on the node")]
    public ListServicesCommand Services { get; init; } = new();

    [Command("start", Description = "Starts a service group")]
    public StartServicesCommand Start { get; init; } = new();

    [Command("stop", Description = "Stops a service group")]
    public StopServicesCommand Stop { get; init; } = new();
}
