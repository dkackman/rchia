using System.ComponentModel;
using Spectre.Console.Cli;

namespace rchia
{
    public class EndpointSettings : CommandSettings
    {
        [Description("The uri of the rpc endpoint, including the proper port and wss/https scheme prefix")]
        [CommandOption("--uri|--endpoint-uri")]
        public string? EndpointUri { get; init; }

        [Description("The full path to the .crt file to use for authentication")]
        [CommandOption("--cp|--cert-path")]
        public string? CertPath { get; init; }

        [Description("The full path to the .key file to use for authentication")]
        [CommandOption("--kp|--key-path")]
        public string? KeyPath { get; init; }

        [Description("The full path to a chia config yaml file for endpoints")]
        [CommandOption("--ccp|--chia-config-path")]
        public string? ConfigPath { get; init; }

        [Description("Flag indicating to use the default chia config for endpoints")]
        [CommandOption("--dcc|--default-chia-config")]
        public bool DefaultConfig { get; init; }

        [Description("Flag indicating to use the default saved endpoint")]
        [CommandOption("--default-endpoint|--de")]
        public bool DefaultEndpoint { get; init; }

        [Description("Use a saved endpoint")]
        [CommandOption("--ep|--endpoint")]
        public string? Endpoint { get; init; }

        [Description("Timeout in seconds")]
        [CommandOption("-t|--timeout")]
        [DefaultValue(30)]
        public int Timeout { get; set; } = 30;

        internal int TimeoutMilliseconds => Timeout * 1000;

        [Description("Set output to verbose messages")]
        [CommandOption("-v|--verbose")]
        public bool Verbose { get; set; }
    }
}
