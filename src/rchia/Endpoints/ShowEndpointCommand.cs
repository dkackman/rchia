using System;

using rchia.Commands;

namespace rchia.Endpoints;

internal sealed class ShowEndpointCommand : Command
{
    [Argument(0, Name = "id", Description = "The id of the endpoint to show")]
    public string Id { get; init; } = string.Empty;

    [CommandTarget]
    public int Run()
    {
        return DoWork("Shoeinh endpoint...", output =>
        {
            var library = EndpointLibrary.OpenLibrary();

            if (!library.Endpoints.ContainsKey(Id))
            {
                throw new InvalidOperationException($"There is no saved endpoint with an id of {Id}.");
            }

            var endpoint = library.Endpoints[Id];
            output.WriteOutput(endpoint);
        });
    }
}
