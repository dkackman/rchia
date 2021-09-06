using System;
using System.Threading.Tasks;

using rchia.Commands;

namespace rchia.Endpoints
{
    internal sealed class ListEndpointsCommand : BaseCommand
    {
        [CommandTarget]
        public override async Task<int> Run()
        {
            try
            {
                var config = Settings.GetConfig();
                var endpointsFilePath = config.endpointfile ?? Settings.DefaultEndpointsFilePath;
                var endpoints = EndpointLibrary.Open(endpointsFilePath);

                EndpointTasks.List(endpoints);

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
