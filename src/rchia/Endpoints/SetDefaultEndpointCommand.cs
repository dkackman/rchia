using System;
using System.Threading.Tasks;

using rchia.Commands;

namespace rchia.Endpoints
{
    internal sealed class SetDefaultEndpointCommand : Command
    {
        [Argument(0, Name = "id", Description = "The id of the endpoint to show")]
        public string Id { get; init; } = string.Empty;

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
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
                MarkupLine($"Endpoint [wheat1]{Id}[/] is now the default");

                await Task.CompletedTask;
            });
        }
    }
}
