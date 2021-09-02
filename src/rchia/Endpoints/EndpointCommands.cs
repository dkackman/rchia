using System;
using System.Collections.Generic;

using chia.dotnet;

namespace rchia.Endpoints
{
    public static class EndpointCommands
    {
        public static void List(IDictionary<string, Endpoint> endpoints)
        {
            Console.WriteLine($"{endpoints.Count} saved endpoint(s)");
            foreach (var endpoint in endpoints.Values)
            {
                var isDefault = endpoint.IsDefault ? "(default)" : string.Empty;
                Console.WriteLine($" - {endpoint.Id} {isDefault}");
            }
        }

        public static Endpoint Add(IDictionary<string, Endpoint> endpoints, string id, string uri, string certPath, string keyPath)
        {
            var endpoint = new Endpoint()
            {
                Id = id,
                EndpointInfo = new EndpointInfo()
                {
                    Uri = new Uri(uri),
                    CertPath = certPath,
                    KeyPath = keyPath
                }
            };

            endpoints.Add(endpoint.Id, endpoint);

            return endpoint;
        }
    }
}
