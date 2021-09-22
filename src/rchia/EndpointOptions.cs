using System;
using System.IO;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;

namespace rchia
{
    public abstract class EndpointOptions : Command
    {
        [Option("uri", "endpoint-uri", ArgumentHelpName = "PATH", Description = "The uri of the rpc endpoint, including the proper port and wss/https scheme prefix")]
        public string? EndpointUri { get; init; }

        [Option("cp", "cert-path", ArgumentHelpName = "PATH", Description = "The full path to the .crt file to use for authentication")]
        public FileInfo? CertPath { get; init; }

        [Option("kp", "key-path", ArgumentHelpName = "PATH", Description = "The full path to the .key file to use for authentication")]
        public FileInfo? KeyPath { get; init; }

        [Option("ccp", "chia-config-path", ArgumentHelpName = "PATH", Description = "The full path to a chia config yaml file for endpoints")]
        public FileInfo? ConfigPath { get; init; }

        [Option("dcc", "default-chia-config", Description = "Flag indicating to use the default chia config for endpoints")]
        public bool DefaultConfig { get; init; }

        [Option("de", "default-endpoint", Description = "Flag indicating to use the default saved endpoint")]
        public bool DefaultEndpoint { get; init; }

        [Option("ep", "endpoint", ArgumentHelpName = "ID", Description = "Use a saved endpoint")]
        public string? Endpoint { get; init; }

        [Option("to", "timeout", Default = 30, ArgumentHelpName = "TIMEOUT", Description = "Timeout in seconds")]
        public int Timeout { get; init; } = 30;

        internal int TimeoutMilliseconds => Timeout * 1000;

        // these next three methods use reflection to find the right constructor. errors won't be caught a compile time
        // should realy be replaced with parameterless constructors and property initialization
        internal protected async Task<TTask> CreateTasksWithDaemon<TTask>(string serviceName) where TTask : ConsoleTask<DaemonProxy>
        {
            var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, serviceName, TimeoutMilliseconds);
            var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);

            var constructor = typeof(TTask).GetConstructor(new Type[] { typeof(DaemonProxy), typeof(IConsoleMessage), typeof(int) });

            return constructor is null
                ? throw new InvalidOperationException($"Cannot create a {typeof(TTask).Name}")
                : constructor.Invoke(new object[] { proxy, this, TimeoutMilliseconds }) is not TTask tasks
                ? throw new InvalidOperationException($"Cannot create a {typeof(TTask).Name}")
                : tasks;
        }

        internal protected async Task<TTask> CreateTasks<TTask, TService>(string serviceName) where TTask : ConsoleTask<TService>
                                                                                              where TService : ServiceProxy
        {
            var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, serviceName, TimeoutMilliseconds);
            var proxy = CreateProxy<TService>(rpcClient, ClientFactory.Factory.OriginService);

            var constructor = typeof(TTask).GetConstructor(new Type[] { typeof(TService), typeof(IConsoleMessage), typeof(int) });

            return constructor is null
                ? throw new InvalidOperationException($"Cannot create a {typeof(TTask).Name}")
                : constructor.Invoke(new object[] { proxy, this, TimeoutMilliseconds }) is not TTask tasks
                ? throw new InvalidOperationException($"Cannot create a {typeof(TTask).Name}")
                : tasks;
        }

        private static TService CreateProxy<TService>(IRpcClient rpcClient, string originService) where TService : ServiceProxy
        {
            var constructor = typeof(TService).GetConstructor(new Type[] { typeof(IRpcClient), typeof(string) });

            return constructor is null
                ? throw new InvalidOperationException($"Cannot create a {typeof(TService).Name}")
                : constructor.Invoke(new object[] { rpcClient, originService }) is not TService proxy
                ? throw new InvalidOperationException($"Cannot create a {typeof(TService).Name}")
                : proxy;
        }
    }
}
