using System;
using System.Threading.Tasks;

using rchia.Commands;

namespace rchia.Endpoints;

internal sealed class TestEndpointCommand : Command
{
    [Argument(0, Name = "id", Description = "The id of the endpoint to test")]
    public string Id { get; init; } = string.Empty;

    [Option("to", "timeout", Default = 30, ArgumentHelpName = "TIMEOUT", Description = "Timeout in seconds")]
    public int Timeout { get; init; } = 30;

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Testing connection...", async output =>
        {
            var library = EndpointLibrary.OpenLibrary();

            if (!library.Endpoints.ContainsKey(Id))
            {
                throw new InvalidCastException($"There is no saved endpoint with an id of {Id}.");
            }

            var endpoint = library.Endpoints[Id];
            await ClientFactory.Factory.TestConnection(endpoint.EndpointInfo, Timeout * 1000);

            output.MarkupLine($"Successfully connected to [wheat1]{Id}[/]");
        });
    }
}
