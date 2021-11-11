using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace rchia
{
    public class EndpointLibrary
    {
        internal static EndpointLibrary OpenLibrary()
        {
            var config = Settings.GetSettings();
            var library = new EndpointLibrary(config.endpointfile ?? Settings.DefaultEndpointsFilePath);
            library.Open();

            return library;
        }

        private readonly string _endpointsFilePath;

        public EndpointLibrary(string endpointsFilePath)
        {
            _endpointsFilePath = endpointsFilePath;
        }

        public Endpoint GetDefault()
        {
            var endpoint = Endpoints.FirstOrDefault(kvp => kvp.Value.IsDefault);

            return endpoint.Value ?? throw new InvalidOperationException("No default endpoint is set. Try './rchia endpoints set-default ID'");
        }

        public IDictionary<string, Endpoint> Endpoints { get; private set; } = new Dictionary<string, Endpoint>();

        public void Open()
        {
            if (File.Exists(_endpointsFilePath))
            {
                try
                {
                    using var reader = new StreamReader(_endpointsFilePath);
                    var json = reader.ReadToEnd();
                    var library = JsonConvert.DeserializeObject<IDictionary<string, Endpoint>>(json);

                    Endpoints = library ?? new Dictionary<string, Endpoint>();
                }
                catch (Exception e)
                {
                    throw new Exception($"The file {_endpointsFilePath} might be corrupt.", e);
                }
            }
        }

        public void Save()
        {
            if (Endpoints.Count == 1)
            {
                Endpoints.Values.First().IsDefault = true;
            }

            var json = JsonConvert.SerializeObject(Endpoints, Formatting.Indented);
            using var writer = new StreamWriter(_endpointsFilePath);
            writer.WriteLine(json);
        }
    }
}
