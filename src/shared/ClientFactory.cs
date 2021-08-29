using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using rchia;
using rchia.Endpoints;

namespace chia.dotnet.console
{
    internal class ClientFactory
    {
        public ClientFactory(string originService)
        {
            OriginService = originService;
        }

        public string OriginService { get; init; }

        public async Task TestConnection(EndpointInfo endpoint)
        {
            using var cts = new CancellationTokenSource(5000);

            using var rpcClient = await CreateRpcClient(endpoint);
        }

        public async Task<IRpcClient> CreateRpcClient(SharedOptions options, string serviceName)
        {
            var endpoint = GetEndpointInfo(options, serviceName);

            return await CreateRpcClient(endpoint);
        }

        public async Task<IRpcClient> CreateRpcClient(EndpointInfo endpoint)
        {
            if (endpoint.Uri.Scheme == "wss")
            {
                using var cts = new CancellationTokenSource(5000);

                var rpcClient = new WebSocketRpcClient(endpoint);
                await rpcClient.Connect(cts.Token);

                var daemon = new DaemonProxy(rpcClient, OriginService);
                await daemon.RegisterService(cts.Token);

                return rpcClient;
            }

            return endpoint.Uri.Scheme == "https"
                ? new HttpRpcClient(endpoint)
                : throw new InvalidOperationException($"Unrecognized endpoint Uri scheme {endpoint.Uri.Scheme}");
        }

        private static EndpointInfo GetEndpointInfo(SharedOptions options, string serviceName)
        {
            if (options.UseDefaultEndpoint)
            {
                var config = Settings.GetConfig();
                var endpointsFilePath = config.endpointfile ?? rchia.Settings.DefaultEndpointsFilePath;
                var endpoint = EndpointLibrary.GetDefault(endpointsFilePath);
                
                return endpoint.EndpointInfo;
            }

            if (!string.IsNullOrEmpty(options.SavedEndpoint))
            {
                var config = rchia.Settings.GetConfig();
                var endpointsFilePath = config.endpointfile ?? rchia.Settings.DefaultEndpointsFilePath;
                IDictionary<string, Endpoint> endpoints = EndpointLibrary.Open(endpointsFilePath);

                if (!endpoints.ContainsKey(options.SavedEndpoint))
                {
                    throw new InvalidOperationException($"There is no saved endpoint {options.SavedEndpoint}");
                }

                return endpoints[options.SavedEndpoint].EndpointInfo;
            }

            if (options.UseDefaultConfig)
            {
                return Config.Open().GetEndpoint(serviceName);
            }

            if (!string.IsNullOrEmpty(options.ConfigPath))
            {
                return Config.Open(options.ConfigPath).GetEndpoint(serviceName);
            }

            return new EndpointInfo()
            {
                Uri = new Uri(options.Uri ?? string.Empty),
                CertPath = options.CertPath ?? string.Empty,
                KeyPath = options.KeyPath ?? string.Empty
            };
        }
    }
}
