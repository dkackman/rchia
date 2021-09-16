using System;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;
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
            using var cts = new CancellationTokenSource(30000);

            using var rpcClient = await CreateRpcClient(null!, endpoint);
        }

        public async Task<IRpcClient> CreateRpcClient(EndpointOptions options, string serviceName)
        {
            var endpoint = GetEndpointInfo(options, serviceName);

            return await options.Status($"Connecting to endpoint {endpoint.Uri}...", async ctx => await CreateRpcClient(ctx, endpoint));
        }

        private async Task<IRpcClient> CreateRpcClient(StatusContext ctx, EndpointInfo endpoint)
        {
            return endpoint.Uri.Scheme == "wss"
                ? await CreateWebSocketClient(ctx, endpoint)
                : endpoint.Uri.Scheme == "https"
                ? new HttpRpcClient(endpoint)
                : throw new InvalidOperationException($"Unrecognized endpoint Uri scheme {endpoint.Uri.Scheme}");
        }

        public async Task<WebSocketRpcClient> CreateWebSocketClient(EndpointOptions options, string serviceName)
        {
            var endpoint = GetEndpointInfo(options, serviceName);

            if (endpoint.Uri.Scheme != "wss")
            {
                throw new InvalidOperationException($"Expecting a daemon endpoint using the websocket protocol but found {endpoint.Uri}");
            }

            return await options.Status($"Connecting to websocket {endpoint.Uri}...", async ctx => await CreateWebSocketClient(ctx, endpoint));
        }

        private async Task<WebSocketRpcClient> CreateWebSocketClient(StatusContext ctx, EndpointInfo endpoint)
        {
            using var cts = new CancellationTokenSource(30000);
            
            var rpcClient = new WebSocketRpcClient(endpoint);
            await rpcClient.Connect(cts.Token);

            var daemon = new DaemonProxy(rpcClient, OriginService);
            await daemon.RegisterService(cts.Token);

            return rpcClient;
        }

        private static EndpointInfo GetEndpointInfo(EndpointOptions options, string serviceName)
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
                options.Message("No endpoint options set. Using default endpoint.");
            }


            var endpoint = EndpointLibrary.GetDefault(endpointsFilePath);
            return endpoint.EndpointInfo;
        }
    }
}
