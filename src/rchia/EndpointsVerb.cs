using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using chia.dotnet.console;

using rchia.Endpoints;

using CommandLine;

namespace rchia
{
    [Verb("endpoints", HelpText = "Manage saved endpoints")]
    internal sealed class EndpointVerb : BaseVerb
    {
        [Option('l', "list", HelpText = "Lists the ids of saved endpoints")]
        public bool List { get; set; }

        [Option('a', "add", HelpText = "[ID] [URI] [CRT PATH] [KEY PATH] Saves a new endpoint")]
        public string? Add { get; set; }

        [Value(0, MetaName = "uri", HelpText = "The Uri of the endpoint being added")]
        public string? Uri { get; set; }

        [Value(1, MetaName = "cert-path", HelpText = "The full path to the .crt file to use for authentication")]
        public string? CertPath { get; set; }

        [Value(2, MetaName = "key-path", HelpText = "The full path to the .key file to use for authentication")]
        public string? KeyPath { get; set; }

        [Option('r', "remove", HelpText = "[ID] Removes a saved endpoint")]
        public string? Remove { get; set; }

        [Option('s', "show", HelpText = "[ID] Shows the details of a saved endpoint")]
        public string? Show { get; set; }

        [Option('d', "set-default", HelpText = "[ID] Sets the endpoint to be the default for --use-default-endpoint")]
        public string? SetDefault { get; set; }

        [Option('t', "test", HelpText = "[ID] Test the connection to a saved endpoint")]
        public string? Test { get; set; }

        public override async Task<int> Run()
        {
            try
            {
                var config = Config.GetConfig();
                var endpointsFilePath = config.endpointfile ?? Config.DefaultEndpointsFilePath;
                IDictionary<string, Endpoint> endpoints = EndpointLibrary.Open(endpointsFilePath);

                if (List)
                {
                    EndpointCommands.List(endpoints);
                }
                else if (!string.IsNullOrEmpty(Add))
                {
                    if (endpoints.ContainsKey(Add))
                    {
                        throw new InvalidOperationException($"An endpoint with an id of {Add} already exists.");
                    }

                    if (string.IsNullOrEmpty(Uri))
                    {
                        throw new InvalidOperationException($"The Uri must be provided in position 0");
                    }

                    if (string.IsNullOrEmpty(CertPath))
                    {
                        throw new InvalidOperationException($"The CertPath must be provided in position 1");
                    }

                    if (string.IsNullOrEmpty(KeyPath))
                    {
                        throw new InvalidOperationException($"The KeyPath must be provided in position 2");
                    }

                    var endpoint = EndpointCommands.Add(endpoints, Add, Uri, CertPath, KeyPath);

                    EndpointLibrary.Save(endpoints, endpointsFilePath);

                    Console.WriteLine($"Added {endpoint.Id}");
                    Console.WriteLine(endpoint);
                }
                else if (!string.IsNullOrEmpty(Remove))
                {
                    if (!endpoints.ContainsKey(Remove))
                    {
                        throw new InvalidOperationException($"No saved endpoint with an id of {Remove}.");
                    }

                    endpoints.Remove(Remove);
                    EndpointLibrary.Save(endpoints, endpointsFilePath);
                    
                    Console.WriteLine($"Removed {Remove}");
                }
                else if (!string.IsNullOrEmpty(Show))
                {
                    if (!endpoints.ContainsKey(Show))
                    {
                        throw new InvalidOperationException($"No saved endpoint with an id of {Show}.");
                    }

                    var endpoint = endpoints[Show];
                    Console.WriteLine(endpoint);
                }
                else if (!string.IsNullOrEmpty(SetDefault))
                {
                    if (!endpoints.ContainsKey(SetDefault))
                    {
                        throw new InvalidOperationException($"No saved endpoint with an id of {SetDefault}.");
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
                        throw new InvalidCastException($"No saved endpoint with an id of {Test}.");
                    }

                    var endpoint = endpoints[Test];
                    await Program.Factory.TestConnection(endpoint.EndpointInfo);

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
