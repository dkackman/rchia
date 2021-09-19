using rchia.Commands;

namespace rchia.Endpoints
{
    [Command("endpoints", Description = "Manage saved endpoints.")]
    internal sealed class EndpointsCommand
    {
        [Command("add", Description = "Saves a new endpoint")]
        public AddEndpointCommand Add { get; set; } = new();

        [Command("list", Description = "Lists the ids of saved endpoints")]
        public ListEndpointsCommand List { get; set; } = new();

        [Command("remove", Description = "Removes a saved endpoint")]
        public RemoveEndpointCommand Remove { get; set; } = new();

        [Command("show", Description = "Shows the details of a saved endpoint")]
        public ShowEndpointCommand Show { get; set; } = new();

        [Command("set-default", Description = "Sets the endpoint to be the default for --default-endpoint")]
        public SetDefaultEndpointCommand SetDefault { get; set; } = new();

        [Command("test", Description = "Test the connection to a saved endpoint")]
        public TestEndpointCommand Test { get; set; } = new();

        internal static EndpointLibrary OpenLibrary()
        {
            var config = Settings.GetConfig();
            var library = new EndpointLibrary(config.endpointfile ?? Settings.DefaultEndpointsFilePath);
            library.Open();

            return library;
        }
    }
}
