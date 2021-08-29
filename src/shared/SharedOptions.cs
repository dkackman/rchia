﻿using CommandLine;

namespace chia.dotnet.console
{
    public abstract class SharedOptions : BaseVerb
    {
        [Option("endpoint-uri", SetName = "Endpoint", HelpText = "[URI] The uri of the rpc endpoint, including the proper port and wss/https scheme prefix")]
        public string? Uri { get; set; }

        [Option("cert-path", SetName = "Endpoint", HelpText = "[PATH] The full path to the .crt file to use for authentication")]
        public string? CertPath { get; set; }

        [Option("key-path", SetName = "Endpoint", HelpText = "[PATH] The full path to the .key file to use for authentication")]
        public string? KeyPath { get; set; }

        [Option("chia-config-path", SetName = "Config", HelpText = "[PATH] The full path to a chia config yaml file for endpoints")]
        public string? ConfigPath { get; set; }

        [Option("default-chia-config", SetName = "Config", HelpText = "Flag indicating to use the default chia config for endpoints")]
        public bool UseDefaultConfig { get; set; }

        [Option("default-endpoint", SetName = "Saved", HelpText = "Flag indicating to use the default saved endpoint")]
        public bool UseDefaultEndpoint { get; set; }

        [Option("saved-endpoint", SetName = "Saved", HelpText = "[ID] Use a saved endpoint")]
        public string? SavedEndpoint { get; set; }
    }
}
