using System;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;

namespace rchia
{
    internal class ClientFactory2
    {
        public static void Initialize(string originService)
        {
            Factory = new(originService);
        }

        internal static ClientFactory2 Factory { get; private set; } = new("not_Set");

        private ClientFactory2(string originService)
        {
            OriginService = originService;
        }

        public string OriginService { get; init; } // this is needed for any daemon wss endpoints

        public async Task TestConnection(EndpointInfo endpoint)
        {
            using var cts = new CancellationTokenSource(30000);

            using var rpcClient = await CreateRpcClient(endpoint);
        }

        public async Task<IRpcClient> CreateRpcClient(EndpointSettings options, string serviceName)
        {
            var endpoint = GetEndpointInfo(options, serviceName);

            //return await options.Status($"Connecting to endpoint {endpoint.Uri}...", async ctx => await CreateRpcClient(endpoint));
            return await CreateRpcClient(endpoint);
        }

        private async Task<IRpcClient> CreateRpcClient(EndpointInfo endpoint)
        {
            return endpoint.Uri.Scheme == "wss"
                ? await CreateWebSocketClient(endpoint)
                : endpoint.Uri.Scheme == "https"
                ? new HttpRpcClient(endpoint)
                : throw new InvalidOperationException($"Unrecognized endpoint Uri scheme {endpoint.Uri.Scheme}");
        }

        public async Task<WebSocketRpcClient> CreateWebSocketClient(EndpointSettings options, string serviceName)
        {
            var endpoint = GetEndpointInfo(options, serviceName);

            if (endpoint.Uri.Scheme != "wss")
            {
                throw new InvalidOperationException($"Expecting a daemon endpoint using the websocket protocol but found {endpoint.Uri}");
            }

           // return await options.Status($"Connecting to websocket {endpoint.Uri}...", async ctx => await CreateWebSocketClient(endpoint));
            return await CreateWebSocketClient(endpoint);
        }

        private async Task<WebSocketRpcClient> CreateWebSocketClient(EndpointInfo endpoint)
        {
            using var cts = new CancellationTokenSource(30000);

            var rpcClient = new WebSocketRpcClient(endpoint);
            await rpcClient.Connect(cts.Token);

            var daemon = new DaemonProxy(rpcClient, OriginService);
            await daemon.RegisterService(cts.Token);

            return rpcClient;
        }

        private static EndpointInfo GetEndpointInfo(EndpointSettings options, string serviceName)
        {
            var library = EndpointLibrary.OpenLibrary();

            if (!string.IsNullOrEmpty(options.Endpoint))
            {
                return !library.Endpoints.ContainsKey(options.Endpoint)
                    ? throw new InvalidOperationException($"There is no saved endpoint {options.Endpoint}")
                    : library.Endpoints[options.Endpoint].EndpointInfo;
            }

            if (options.DefaultConfig)
            {
                return Config.Open().GetEndpoint(serviceName);
            }

            if (options.ConfigPath is not null)
            {
                return Config.Open(options.ConfigPath).GetEndpoint(serviceName);
            }

            if (!string.IsNullOrEmpty(options.EndpointUri))
            {
                return options.CertPath is null || options.KeyPath is null
                    ? throw new InvalidOperationException("When a --endpoint-uri is set both --key-path and --cert-path must also be set")
                    : new EndpointInfo()
                    {
                        Uri = new Uri(options.EndpointUri),
                        CertPath = options.CertPath,
                        KeyPath = options.KeyPath
                    };
            }

            if (!options.DefaultEndpoint)
            {
                options.Helpful("No endpoint options set. Using default endpoint.");
            }

            var endpoint = library.GetDefault();
            return endpoint.EndpointInfo;
        }
    }
}
