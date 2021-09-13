using System;
using System.Threading.Tasks;

using rchia.Commands;

namespace rchia.Endpoints
{
    internal sealed class TestEndpointCommand : Command
    {
        [Argument(0, Name = "id", Description = "The id of the endpoint to show")]
        public string Id { get; set; } = string.Empty;

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                var config = Settings.GetConfig();
                var endpointsFilePath = config.endpointfile ?? Settings.DefaultEndpointsFilePath;
                var endpoints = EndpointLibrary.Open(endpointsFilePath);

                if (!endpoints.ContainsKey(Id))
                {
                    throw new InvalidCastException($"There is no saved endpoint with an id of {Id}.");
                }

                var endpoint = endpoints[Id];
                await ClientFactory.Factory.TestConnection(endpoint.EndpointInfo);

                Console.WriteLine($"Successfully connected to {Id}");

                await Task.CompletedTask;
            });
        }
    }
}
