using System;
using System.IO;
using System.Linq;
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

        // these next two methods use reflection to find the right constructor. errors won't be caught a compile time
        // should realy be replaced with parameterless constructors and property initialization
        internal protected async Task<TTask> CreateTasksWithDaemon<TTask>(string serviceName) where TTask : ConsoleTask<DaemonProxy>
        {
            var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, serviceName);
            var proxy = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);

            return Create<TTask>(proxy, this, TimeoutMilliseconds);
        }

        internal protected async Task<TTask> CreateTasks<TTask, TService>(string serviceName) where TTask : ConsoleTask<TService>
                                                                                              where TService : ServiceProxy
        {
            var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, serviceName);
            var proxy = Create<TService>(rpcClient, ClientFactory.Factory.OriginService);

            return Create<TTask>(proxy, this, TimeoutMilliseconds);
        }

        private static T Create<T>(params object[] args)
        {
            var constructor = typeof(T).GetConstructor(args.Select(arg => arg.GetType()).ToArray());

            return constructor is null || constructor.Invoke(args) is not T retval
                ? throw new InvalidOperationException($"Cannot create a {typeof(T).Name}")
                : retval;
        }
    }
}
