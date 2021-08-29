using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace chia.dotnet.console
{
    internal class ClientFactory
    {
        public static void Initialize(string originService)
        {
            Factory = new(originService);
        }

        internal static ClientFactory Factory { get; private set; } = new("not_Set");

        private ClientFactory(string originService)
        {
            OriginService = originService;
        }

        public string OriginService { get; init; } // this is needed for any daemon wss endpoints

        public async Task TestConnection(EndpointInfo endpoint)
        {
            using var cts = new CancellationTokenSource(5000);

            using var rpcClient = await CreateRpcClient(endpoint);
        }

        public async Task<IRpcClient> CreateRpcClient(SharedOptions options, string serviceName)
        {
            var endpoint = GetEndpointInfo(options, serviceName);

            options.Message($"Using endpoint {endpoint.Uri}");

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
                var endpointsFilePath = config.endpointfile ?? Settings.DefaultEndpointsFilePath;
                var endpoint = EndpointLibrary.GetDefault(endpointsFilePath);

                return endpoint.EndpointInfo;
            }

            if (!string.IsNullOrEmpty(options.SavedEndpoint))
            {
                var config = Settings.GetConfig();
                var endpointsFilePath = config.endpointfile ?? Settings.DefaultEndpointsFilePath;
                IDictionary<string, Endpoint> endpoints = EndpointLibrary.Open(endpointsFilePath);

                return !endpoints.ContainsKey(options.SavedEndpoint)
                    ? throw new InvalidOperationException($"There is no saved endpoint {options.SavedEndpoint}")
                    : endpoints[options.SavedEndpoint].EndpointInfo;
            }

            return options.UseDefaultConfig
                ? Config.Open().GetEndpoint(serviceName)
                : !string.IsNullOrEmpty(options.ConfigPath)
                ? Config.Open(options.ConfigPath).GetEndpoint(serviceName)
                : new EndpointInfo()
                {
                    Uri = new Uri(options.Uri ?? string.Empty),
                    CertPath = options.CertPath ?? string.Empty,
                    KeyPath = options.KeyPath ?? string.Empty
                };
        }
    }
}
