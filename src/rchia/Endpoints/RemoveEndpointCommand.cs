using System;
using rchia.Commands;

namespace rchia.Endpoints;

internal sealed class RemoveEndpointCommand : Command
{
    [Argument(0, Name = "id", Description = "The id of the endpoint to remove")]
    public string Id { get; init; } = string.Empty;

    [CommandTarget]
    public int Run()
    {
        return DoWork("Removing endpoint...", output =>
        {
            var library = EndpointLibrary.OpenLibrary();

            if (!library.Endpoints.ContainsKey(Id))
            {
                throw new InvalidOperationException($"There is no saved endpoint with an id of {Id}.");
            }

            _ = library.Endpoints.Remove(Id);
            library.Save();

            output.WriteOutput("removed", Id, true);
        });
    }
}
