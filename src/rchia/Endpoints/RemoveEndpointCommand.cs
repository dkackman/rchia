using System;
using System.Threading.Tasks;

using rchia.Commands;

namespace rchia.Endpoints
{
    internal sealed class RemoveEndpointCommand : Command
    {
        [Argument(0, Name = "id", Description = "The id of the endpoint to remove")]
        public string Id { get; set; } = string.Empty;

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

                _ = library.Endpoints.Remove(Id);
                library.Save();

                MarkupLine($"Endpoint [wheat1]{Id}[/] removed");

                await Task.CompletedTask;
            });
        }
    }
}
