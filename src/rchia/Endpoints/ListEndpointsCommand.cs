using rchia.Commands;

namespace rchia.Endpoints;

internal sealed class ListEndpointsCommand : Command
{
    [CommandTarget]
    public int Run()
    {
        return DoWork("Listing saved endpoints...", output =>
        {
            var library = EndpointLibrary.OpenLibrary();
            foreach (var endpoint in library.Endpoints.Values)
            {
                var isDefault = endpoint.IsDefault ? "[wheat1](default)[/]" : string.Empty;
                output.MarkupLine($" - {endpoint.Id} {isDefault}");
            }

            if (Json)
            {
                output.WriteOutput(library.Endpoints.Values);
            }

            output.MarkupLine($"[wheat1]{library.Endpoints.Count}[/] saved endpoint{(library.Endpoints.Count == 1 ? string.Empty : "s")}");
        });
    }
}
