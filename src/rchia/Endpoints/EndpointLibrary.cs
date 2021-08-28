using System;
using System.Collections.Generic;
using System.IO;

using chia.dotnet;

using Newtonsoft.Json;

namespace rchia.Endpoints
{
    public static class EndpointLibrary
    {
        public static IDictionary<string, Endpoint> Open(string filepath)
        {
            return new Dictionary<string, Endpoint>();
        }

        public static void Save(IDictionary<string, Endpoint> endpoints)
        {

        }
    }
}
