using CommandLine;

namespace chia.dotnet.console
{
    public abstract class SharedOptions : BaseVerb
    {
        [Option("endpoint-uri", SetName = "Endpoint", HelpText = "The uri of the rpc endpoint, including the proper port and wss/https scheme prefix")]
        public string? Uri { get; set; }

        [Option("cert-path", SetName = "Endpoint", HelpText = "The full path to the .crt file to use for authentication")]
        public string? CertPath { get; set; }

        [Option("key-path", SetName = "Endpoint", HelpText = "The full path to the .key file to use for authentication")]
        public string? KeyPath { get; set; }

        [Option("config-path", SetName = "Config", HelpText = "The full path to a chia config yaml file for endpoints")]
        public string? ConfigPath { get; set; }

        [Option("use-default-config", SetName = "Config", HelpText = "Flag indicating to use the default chia config for endpoints")]
        public bool UseDefaultConfig { get; set; }
    }
}
