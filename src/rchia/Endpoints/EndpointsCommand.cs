using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using rchia.Commands;

namespace rchia.Endpoints
{
    [Command("endpoints", Description = "Manage saved endpoints.")]
    internal sealed class EndpointCommand : BaseCommand
    {
        [Option('l', "list", Description = "Lists the ids of saved endpoints")]
        public bool List { get; set; }

        [Command("add", Description = "Saves a new endpoint")]
        public AddCommand Add { get; set; } = new();

        [Option('r', "remove", ArgumentHelpName = "ID", Description = "Removes a saved endpoint")]
        public string? Remove { get; set; }

        [Option('s', "show", ArgumentHelpName = "ID", Description = "Shows the details of a saved endpoint")]
        public string? Show { get; set; }

        [Option('d', "set-default", ArgumentHelpName = "ID", Description = "Sets the endpoint to be the default for --default-endpoint")]
        public string? SetDefault { get; set; }

        [Option('t', "test", ArgumentHelpName = "ID", Description = "Test the connection to a saved endpoint")]
        public string? Test { get; set; }

        public override async Task<int> Run()
        {
            try
            {
                var config = Settings.GetConfig();
                var endpointsFilePath = config.endpointfile ?? Settings.DefaultEndpointsFilePath;
                IDictionary<string, Endpoint> endpoints = EndpointLibrary.Open(endpointsFilePath);

                if (List)
                {
                    EndpointCommands.List(endpoints);
                }
                else if (!string.IsNullOrEmpty(Remove))
                {
                    if (!endpoints.ContainsKey(Remove))
                    {
                        throw new InvalidOperationException($"There is no saved endpoint with an id of {Remove}.");
                    }

                    _ = endpoints.Remove(Remove);
                    EndpointLibrary.Save(endpoints, endpointsFilePath);

                    Console.WriteLine($"Endpoint {Remove} removed");
                }
                else if (!string.IsNullOrEmpty(Show))
                {
                    if (!endpoints.ContainsKey(Show))
                    {
                        throw new InvalidOperationException($"There is no saved endpoint with an id of {Show}.");
                    }

                    var endpoint = endpoints[Show];
                    Console.WriteLine(endpoint);
                }
                else if (!string.IsNullOrEmpty(SetDefault))
                {
                    if (!endpoints.ContainsKey(SetDefault))
                    {
                        throw new InvalidOperationException($"There is no saved endpoint with an id of {SetDefault}.");
                    }

                    foreach (var endpoint in endpoints.Values)
                    {
                        endpoint.IsDefault = endpoint.Id == SetDefault;
                    }

                    EndpointLibrary.Save(endpoints, endpointsFilePath);
                    Console.WriteLine($"Endpoint {SetDefault} is now the default");
                }
                else if (!string.IsNullOrEmpty(Test))
                {
                    if (!endpoints.ContainsKey(Test))
                    {
                        throw new InvalidCastException($"There is no saved endpoint with an id of {Test}.");
                    }

                    var endpoint = endpoints[Test];
                    await ClientFactory.Factory.TestConnection(endpoint.EndpointInfo);

                    Console.WriteLine($"Successfully connected to {Test}");
                }

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
