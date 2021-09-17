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
                var config = Settings.GetConfig();
                var endpointsFilePath = config.endpointfile ?? Settings.DefaultEndpointsFilePath;
                var endpoints = EndpointLibrary.Open(endpointsFilePath);

                MarkupLine($"[bold]{endpoints.Count}[/] saved endpoint(s)");
                foreach (var endpoint in endpoints.Values)
                {
                    var isDefault = endpoint.IsDefault ? "[bold](default)[/]" : string.Empty;
                    MarkupLine($" - {endpoint.Id} {isDefault}");
                }

                await Task.CompletedTask;
            });
        }
    }
}
