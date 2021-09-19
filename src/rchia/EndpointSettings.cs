using System;
using System.Diagnostics;
using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace rchia
{
    public class EndpointSettings : CommandSettings, IConsoleMessage
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

        [Description("Set output to verbose messages")]
        [CommandOption("-v|--verbose")]
        public bool Verbose { get; set; }

        public void MarkupLine(string msg)
        {
            AnsiConsole.MarkupLine(msg);
        }

        public void WriteLine(string msg)
        {
            AnsiConsole.WriteLine(msg);
        }

        public void Message(string msg, bool important = false)
        {
            if (important)
            {
                AnsiConsole.MarkupLine($"[yellow]{msg}[/]");
            }
            else if (Verbose)
            {
                AnsiConsole.MarkupLine(msg);
            }

            Debug.WriteLine(msg);
        }

        public void NameValue(string name, object? value)
        {
            AnsiConsole.MarkupLine($"[wheat1]{name}:[/] {value}");
        }

        public void Helpful(string msg, bool important = false)
        {
            if (Verbose || important)
            {
                AnsiConsole.MarkupLine($"[grey]{msg}[/]");
            }
        }

        public void Warning(string msg)
        {
            AnsiConsole.MarkupLine($"[yellow]{msg}[/]");
        }

        public void Message(Exception e)
        {
            if (Verbose)
            {
                AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks);
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]{e.Message}[/]");
            }
        }

        public bool Confirm(string warning, bool force)
        {
            if (!force)
            {
                if (!AnsiConsole.Confirm(warning, false))
                {
                    Message("Cancelled");
                    return false;
                }
                Message("Confirmed");
            }

            return true;
        }
    }
}
