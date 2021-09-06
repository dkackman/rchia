using System;
using System.Threading.Tasks;

using rchia.Commands;

namespace rchia.Endpoints
{
    internal sealed class ShowEndpointCommand : BaseCommand
    {
        [Argument(0, Name = "id", Description = "The id of the endpoint to show")]
        public string Id { get; set; } = string.Empty;

        [CommandTarget]
        public override async Task<int> Run()
        {
            try
            {
                var config = Settings.GetConfig();
                var endpointsFilePath = config.endpointfile ?? Settings.DefaultEndpointsFilePath;
                var endpoints = EndpointLibrary.Open(endpointsFilePath);

                if (!endpoints.ContainsKey(Id))
                {
                    throw new InvalidOperationException($"There is no saved endpoint with an id of {Id}.");
                }

                var endpoint = endpoints[Id];
                Console.WriteLine(endpoint);

                await Task.CompletedTask;
                return 0;
            }
            catch (Exception e)
            {
                Message(e);

                return -1;
            }
        }
    }
}
