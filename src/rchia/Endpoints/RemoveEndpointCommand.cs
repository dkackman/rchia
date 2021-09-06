using System;
using System.Threading.Tasks;

using rchia.Commands;

namespace rchia.Endpoints
{
    internal sealed class RemoveEndpointCommand : BaseCommand
    {
        [Argument(0, Name = "id", Description = "The id of the endpoint to remove")]
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

                _ = endpoints.Remove(Id);
                EndpointLibrary.Save(endpoints, endpointsFilePath);

                Console.WriteLine($"Endpoint {Id} removed");

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
