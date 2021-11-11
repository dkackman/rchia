using rchia.Commands;

namespace rchia.Endpoints
{
    [Command("endpoints", Description = "Manage saved endpoints.")]
    internal sealed class EndpointsCommands
    {
        [Command("add", Description = "Saves a new endpoint")]
        public AddEndpointCommand Add { get; init; } = new();

        [Command("list", Description = "Lists the ids of saved endpoints")]
        public ListEndpointsCommand List { get; init; } = new();

        [Command("remove", Description = "Removes a saved endpoint")]
        public RemoveEndpointCommand Remove { get; init; } = new();

        [Command("show", Description = "Shows the details of a saved endpoint")]
        public ShowEndpointCommand Show { get; init; } = new();

        [Command("set-default", Description = "Sets the endpoint to be the default for --default-endpoint")]
        public SetDefaultEndpointCommand SetDefault { get; init; } = new();

        [Command("test", Description = "Test the connection to a saved endpoint")]
        public TestEndpointCommand Test { get; init; } = new();
    }
}
