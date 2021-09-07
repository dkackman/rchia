using System;
using System.Threading;
using System.Threading.Tasks;

using chia.dotnet;

namespace rchia.Endpoints
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
            using var cts = new CancellationTokenSource(20000);

            using var rpcClient = await CreateRpcClient(endpoint);
        }

        public async Task<IRpcClient> CreateRpcClient(SharedOptions options, string serviceName)
        {
            var endpoint = GetEndpointInfo(options, serviceName);

            options.Message($"Using endpoint {endpoint.Uri}");

            return await CreateRpcClient(endpoint);
        }

        public async Task<WebSocketRpcClient> CreateWebSocketClient(SharedOptions options, string serviceName)
        {
            var endpoint = GetEndpointInfo(options, serviceName);

            if (endpoint.Uri.Scheme != "wss")
            {
                throw new InvalidOperationException($"Expecting a daemon endpoint using the websocket protocol but found {endpoint.Uri}");
            }

            options.Message($"Using endpoint {endpoint.Uri}");

            return await CreateWebSocketClient(endpoint);
        }

        private async Task<WebSocketRpcClient> CreateWebSocketClient(EndpointInfo endpoint)
        {
            using var cts = new CancellationTokenSource(20000);

            var rpcClient = new WebSocketRpcClient(endpoint);
            await rpcClient.Connect(cts.Token);

            var daemon = new DaemonProxy(rpcClient, OriginService);
            await daemon.RegisterService(cts.Token);

            return rpcClient;
        }

        public async Task<IRpcClient> CreateRpcClient(EndpointInfo endpoint)
        {
            return endpoint.Uri.Scheme == "wss"
                ? await CreateWebSocketClient(endpoint)
                : endpoint.Uri.Scheme == "https"
                ? new HttpRpcClient(endpoint)
                : throw new InvalidOperationException($"Unrecognized endpoint Uri scheme {endpoint.Uri.Scheme}");
        }

        private static EndpointInfo GetEndpointInfo(SharedOptions options, string serviceName)
        {
            var config = Settings.GetConfig();
            var endpointsFilePath = config.endpointfile ?? Settings.DefaultEndpointsFilePath;

            if (!string.IsNullOrEmpty(options.Endpoint))
            {
                var endpoints = EndpointLibrary.Open(endpointsFilePath);

                return !endpoints.ContainsKey(options.Endpoint)
                    ? throw new InvalidOperationException($"There is no saved endpoint {options.Endpoint}")
                    : endpoints[options.Endpoint].EndpointInfo;
            }

            if (options.DefaultConfig)
            {
                return Config.Open().GetEndpoint(serviceName);
            }

            if (options.ConfigPath is not null)
            {
                return Config.Open(options.ConfigPath.FullName).GetEndpoint(serviceName);
            }

            if (!string.IsNullOrEmpty(options.EndpointUri))
            {
                return options.CertPath is null || options.KeyPath is null
                    ? throw new InvalidOperationException("When a --endpoint-uri is set both --key-path and --cert-path must also be set")
                    : new EndpointInfo()
                    {
                        Uri = new Uri(options.EndpointUri),
                        CertPath = options.CertPath.FullName,
                        KeyPath = options.KeyPath.FullName
                    };
            }

            if (!options.DefaultEndpoint)
            {
                options.Message("No endpoint opions set. Using default endpoint.");
            }


            var endpoint = EndpointLibrary.GetDefault(endpointsFilePath);
            return endpoint.EndpointInfo;
        }
    }
}
