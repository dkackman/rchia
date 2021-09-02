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
            using var cts = new CancellationTokenSource(5000);

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
            using var cts = new CancellationTokenSource(5000);

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
            if (options.DefaultEndpoint)
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
                var endpoints = EndpointLibrary.Open(endpointsFilePath);

                return !endpoints.ContainsKey(options.SavedEndpoint)
                    ? throw new InvalidOperationException($"There is no saved endpoint {options.SavedEndpoint}")
                    : endpoints[options.SavedEndpoint].EndpointInfo;
            }

            return options.DefaultConfig
                ? Config.Open().GetEndpoint(serviceName)
                : options.ConfigPath is not null
                ? Config.Open(options.ConfigPath.FullName).GetEndpoint(serviceName)
                : new EndpointInfo()
                {
                    Uri = new Uri(options.EndpointUri ?? string.Empty),
                    CertPath = options.CertPath?.FullName ?? string.Empty,
                    KeyPath = options.KeyPath?.FullName ?? string.Empty
                };
        }
    }
}
