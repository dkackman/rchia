﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace chia.dotnet.console.Endpoints
{
    internal sealed class AddCommand : BaseVerb
    {
        [Argument(0, Name = "id", Description = "The id of the endpoint being added")]
        public string Id { get; set; } = string.Empty;

        [Argument(1, Name = "uri", Description = "The Uri of the endpoint being added")]
        public string Uri { get; set; } = string.Empty;

        [Argument(2, Name = "cert-path", Description = "The full path to the .crt file to use for authentication")]
        public FileInfo? CertPath { get; set; }

        [Argument(3, Name = "key-path", Description = "The full path to the .key file to use for authentication")]
        public FileInfo? KeyPath { get; set; }

        public override async Task<int> Run()
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

                var endpoint = EndpointCommands.Add(endpoints, Id, Uri, CertPath.FullName, KeyPath.FullName);

                EndpointLibrary.Save(endpoints, endpointsFilePath);

                Console.WriteLine($"Endpoint {endpoint.Id} added");
                Console.WriteLine(endpoint);

                await Task.CompletedTask;
                return 0;
            }

            return -1;
        }
    }
}
