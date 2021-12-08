using rchia.Commands;

namespace rchia.Services;

[Command("services", Description = "Shows the status, start and stop services running on the node.\nRequires a daemon endpoint.")]
internal sealed class ServicesCommands
{
    [Command("list", Description = "Show which services are running on the node")]
    public ListServicesCommand Services { get; init; } = new();

    [Command("start", Description = "Starts a service group")]
    public StartServicesCommand Start { get; init; } = new();

    [Command("stop", Description = "Stops a service group")]
    public StopServicesCommand Stop { get; init; } = new();
}
