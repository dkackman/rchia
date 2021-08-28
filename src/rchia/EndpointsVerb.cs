using System;
using System.Threading.Tasks;

using chia.dotnet.console;

using rchia.Endpoints;

using CommandLine;

namespace rchia
{
    [Verb("endpoints", HelpText = "Manage saved endpoints")]
    internal sealed class EndpointVerb : BaseVerb
    {
        [Option('l', "list", HelpText = "Show the list of saved endpoints")]
        public bool List { get; set; }

        [Option('a', "add", HelpText = "[ID] [URI] [CRT PATH] [KEY PATH] Adds a saved endpoint")]
        public string? Add { get; set; }

        [Option('r', "remove", HelpText = "[ID] Removes a saved endpoint")]
        public string? Remove { get; set; }

        [Option('s', "show", HelpText = "[ID] Shows the details of a saved endpoint")]
        public string? Show { get; set; }

        [Option('t', "test", HelpText = "[ID] Test the connection to a saved endpoint")]
        public string? Test { get; set; }

        public override async Task<int> Run()
        {
            try
            {
                var config = Config.GetConfig();
                var endpointsfile = config.endpointfile ?? Config.DefaultEndpointsFilePath;
                var endpoints = EndpointLibrary.Open(endpointsfile);

                if (List)
                {
                    foreach (var endpoint in endpoints)
                    {

                    }
                }
                else if (!string.IsNullOrEmpty(Add))
                {
                    var endpoint = new Endpoint();
                    if (endpoints.ContainsKey(endpoint.Id))
                    {
                        throw new InvalidOperationException($"An endpoint with an id of {Add} already exists.");
                    }
                    endpoints.Add(endpoint.Id, endpoint);
                    EndpointLibrary.Save(endpoints);
                }
                else if (!string.IsNullOrEmpty(Remove))
                {
                    if (!endpoints.ContainsKey(Remove))
                    {
                        throw new InvalidOperationException($"No saved endpoint with an id of {Remove}.");
                    }
                    endpoints.Remove(Remove);
                    EndpointLibrary.Save(endpoints);
                }
                else if (!string.IsNullOrEmpty(Show))
                {
                    if (!endpoints.ContainsKey(Show))
                    {
                        throw new InvalidOperationException($"No saved endpoint with an id of {Remove}.");
                    }
                }
                else if (!string.IsNullOrEmpty(Test))
                {
                    if (!endpoints.ContainsKey(Test))
                    {
                        throw new InvalidCastException($"No saved endpoint with an id of {Remove}.");
                    }
                }
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
