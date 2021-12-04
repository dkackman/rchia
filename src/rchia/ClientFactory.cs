using System;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using Spectre.Console;

namespace rchia
{
    internal class ClientFactory
    {
        public static void Initialize(string originService)
        {
            Factory = new(originService);
        }

        public static ClientFactory Factory { get; private set; } = new("not_set");

        private ClientFactory(string originService)
        {
            OriginService = originService;
        }

        public string OriginService { get; init; } // this is needed for any daemon wss endpoints

        public async Task TestConnection(EndpointInfo endpoint, int timeoutMilliseconds)
        {
            using var rpcClient = await CreateRpcClient(endpoint, timeoutMilliseconds);
        }

        public async Task<IRpcClient> CreateRpcClient(IStatus status, EndpointOptions options, string serviceName)
        {
            var endpoint = GetEndpointInfo(options, serviceName);
            using var message = new StatusMessage(status, $"Connecting to endpoint {endpoint.Uri}...");

            return await CreateRpcClient(endpoint, options.TimeoutMilliseconds);
        }

        private async Task<IRpcClient> CreateRpcClient(EndpointInfo endpoint, int timeoutMilliseconds)
        {
            return endpoint.Uri.Scheme == "wss"
                ? await CreateWebSocketClient(endpoint, timeoutMilliseconds)
                : endpoint.Uri.Scheme == "https"
                ? new HttpRpcClient(endpoint)
                : throw new InvalidOperationException($"Unrecognized endpoint Uri scheme {endpoint.Uri.Scheme}");
        }

        public async Task<WebSocketRpcClient> CreateWebSocketClient(IStatus status, EndpointOptions options)
        {
            var endpoint = GetEndpointInfo(options, ServiceNames.Daemon);

            if (endpoint.Uri.Scheme != "wss")
            {
                throw new InvalidOperationException($"Expecting a daemon endpoint using the websocket protocol but found {endpoint.Uri}");
            }
            using var message = new StatusMessage(status, $"Connecting to websocket {endpoint.Uri}...");

            return await CreateWebSocketClient(endpoint, options.TimeoutMilliseconds);
        }

        private async Task<WebSocketRpcClient> CreateWebSocketClient(EndpointInfo endpoint, int timeoutMilliseconds)
        {
            using var cts = new CancellationTokenSource(timeoutMilliseconds);

            var rpcClient = new WebSocketRpcClient(endpoint);
            await rpcClient.Connect(cts.Token);

            var daemon = new DaemonProxy(rpcClient, OriginService);
            await daemon.RegisterService(cts.Token);

            return rpcClient;
        }

        private static EndpointInfo GetEndpointInfo(EndpointOptions options, string serviceName)
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
                options.Helpful("No endpoint options set. Using default endpoint.");
            }

            var endpoint = library.GetDefault();
            return endpoint.EndpointInfo;
        }
    }
}
