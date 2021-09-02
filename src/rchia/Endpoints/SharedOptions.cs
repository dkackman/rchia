using System.IO;

using rchia.Commands;

namespace rchia.Endpoints
{
    public abstract class SharedOptions : BaseCommand
    {
        [Option("endpoint-uri", ArgumentHelpName = "PATH", Description = "The uri of the rpc endpoint, including the proper port and wss/https scheme prefix")]
        public string? EndpointUri { get; set; }

        [Option("cert-path", ArgumentHelpName = "PATH", Description = "The full path to the .crt file to use for authentication")]
        public FileInfo? CertPath { get; set; }

        [Option("key-path", ArgumentHelpName = "PATH", Description = "The full path to the .key file to use for authentication")]
        public FileInfo? KeyPath { get; set; }

        [Option("cconfig-path", ArgumentHelpName = "PATH", Description = "The full path to a chia config yaml file for endpoints")]
        public FileInfo? ConfigPath { get; set; }

        [Option("default-config", Description = "Flag indicating to use the default chia config for endpoints")]
        public bool DefaultConfig { get; set; }

        [Option("default-endpoint", Description = "Flag indicating to use the default saved endpoint")]
        public bool DefaultEndpoint { get; set; }

        [Option("saved-endpoint", ArgumentHelpName = "ID", Description = "Use a saved endpoint")]
        public string? SavedEndpoint { get; set; }
    }
}
