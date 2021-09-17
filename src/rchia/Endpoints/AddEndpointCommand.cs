using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Endpoints
{
    internal sealed class AddEndpointCommand : Command
    {
        [Argument(0, Name = "id", Description = "The id of the endpoint being added")]
        public string Id { get; set; } = string.Empty;

        [Argument(1, Name = "uri", Description = "The Uri of the endpoint being added")]
        public string Uri { get; set; } = string.Empty;

        [Argument(2, Name = "cert-path", Description = "The full path to the .crt file to use for authentication")]
        public FileInfo? CertPath { get; set; }

        [Argument(3, Name = "key-path", Description = "The full path to the .key file to use for authentication")]
        public FileInfo? KeyPath { get; set; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                var config = Settings.GetConfig();
                var endpointsFilePath = config.endpointfile ?? Settings.DefaultEndpointsFilePath;
                IDictionary<string, Endpoint> endpoints = EndpointLibrary.Open(endpointsFilePath);

                if (!string.IsNullOrEmpty(Id))
                {
                    if (endpoints.ContainsKey(Id))
                    {
                        throw new InvalidOperationException($"An endpoint with an id of {Id} already exists.");
                    }

                    if (string.IsNullOrEmpty(Uri))
                    {
                        throw new InvalidOperationException($"The Uri must be provided in position 0");
                    }

                    if (CertPath is null)
                    {
                        throw new InvalidOperationException($"The CertPath must be provided in position 1");
                    }

                    if (KeyPath is null)
                    {
                        throw new InvalidOperationException($"The KeyPath must be provided in position 2");
                    }

                    var endpoint = new Endpoint()
                    {
                        Id = Id,
                        EndpointInfo = new EndpointInfo()
                        {
                            Uri = new Uri(Uri),
                            CertPath = CertPath.FullName,
                            KeyPath = KeyPath.FullName
                        }
                    };

                    endpoints.Add(endpoint.Id, endpoint);

                    EndpointLibrary.Save(endpoints, endpointsFilePath);

                    MarkupLine($"Endpoint [bold]{endpoint.Id}[/] added");
                    WriteLine(endpoint.ToJson());

                    await Task.CompletedTask;
                }
            });
        }
    }
}
