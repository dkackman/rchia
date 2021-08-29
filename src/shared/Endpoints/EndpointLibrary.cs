using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

namespace chia.dotnet.console
{
    public static class EndpointLibrary
    {

        public static Endpoint GetDefault(string endpointsFilePath)
        {
            var endpoints = Open(endpointsFilePath);
            var endpoint = endpoints.FirstOrDefault(kvp => kvp.Value.IsDefault);

            return endpoint.Value ?? throw new InvalidOperationException("No default endpoint is set. Try ./rchia endpoints --set-default NAME");
        }

        public static IDictionary<string, Endpoint> Open(string filepath)
        {
            if (File.Exists(filepath))
            {
                using var reader = new StreamReader(filepath);
                var json = reader.ReadToEnd();
                var library = JsonConvert.DeserializeObject<IDictionary<string, Endpoint>>(json);

                return library ?? new Dictionary<string, Endpoint>();
            }

            return new Dictionary<string, Endpoint>();
        }

        public static void Save(IDictionary<string, Endpoint> endpoints, string filepath)
        {
            if (endpoints.Count == 1)
            {
                endpoints.Values.First().IsDefault = true;
            }

            var json = JsonConvert.SerializeObject(endpoints, Formatting.Indented);
            using var writer = new StreamWriter(filepath);
            writer.WriteLine(json);
        }
    }
}
