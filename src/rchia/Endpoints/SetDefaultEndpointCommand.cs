using System;

using rchia.Commands;

namespace rchia.Endpoints;

internal sealed class SetDefaultEndpointCommand : Command
{
    [Argument(0, Name = "id", Description = "The id of the endpoint to show")]
    public string Id { get; init; } = string.Empty;

    [CommandTarget]
    public int Run()
    {
        return DoWork("Setting default endpoint...", output =>
        {
            var library = EndpointLibrary.OpenLibrary();

            if (!library.Endpoints.ContainsKey(Id))
            {
                throw new InvalidOperationException($"There is no saved endpoint with an id of {Id}.");
            }

            foreach (var endpoint in library.Endpoints.Values)
            {
                endpoint.IsDefault = endpoint.Id == Id;
            }

            library.Save();
            output.MarkupLine($"Endpoint [wheat1]{Id}[/] is now the default");
        });
    }
}
