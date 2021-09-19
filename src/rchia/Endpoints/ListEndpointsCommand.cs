using System.Threading.Tasks;

using rchia.Commands;

namespace rchia.Endpoints
{
    internal sealed class ListEndpointsCommand : Command
    {
        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                var library = EndpointsCommand.OpenLibrary();

                foreach (var endpoint in library.Endpoints.Values)
                {
                    var isDefault = endpoint.IsDefault ? "[wheat1](default)[/]" : string.Empty;
                    MarkupLine($" - {endpoint.Id} {isDefault}");
                }

                MarkupLine($"[wheat1]{library.Endpoints.Count}[/] saved endpoint{(library.Endpoints.Count == 1 ? string.Empty : "s")}");
                await Task.CompletedTask;
            });
        }
    }
}
