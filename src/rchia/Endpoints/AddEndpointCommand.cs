using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using chia.dotnet;
using Spectre.Console;
using Spectre.Console.Cli;

namespace rchia.Endpoints
{
    [Description("Adds a saved endpoint")]
    internal sealed class AddEndpointCommand : Command<AddEndpointCommand.AddEndpointSettings>
    {
        public class AddEndpointSettings : EndpointIdCommandSettings
        {
            [Description("The Uri of the endpoint being added")]
            [CommandArgument(1, "<uri>")]
            public string Uri { get; set; } = string.Empty;

            [Description("The full path to the .crt file to use for authentication")]
            [CommandArgument(2, "<cert-path>")]

            public string CertPath { get; init; } = string.Empty;

            [Description("The full path to the .key file to use for authentication")]
            [CommandArgument(3, "<key-path>")]
            public string KeyPath { get; init; } = string.Empty;

            public override ValidationResult Validate()
            {
                Library = EndpointLibrary.OpenLibrary();

                if (Library.Endpoints.ContainsKey(Id))
                {
                    return ValidationResult.Error($"An endpoint with an id of {Id} already exists.");
                }

                return ValidationResult.Success();
            }
        }

        public override int Execute([NotNull] CommandContext context, [NotNull] AddEndpointSettings settings)
        {
            var endpoint = new Endpoint()
            {
                Id = settings.Id,
                EndpointInfo = new EndpointInfo()
                {
                    Uri = new Uri(settings.Uri),
                    CertPath = settings.CertPath,
                    KeyPath = settings.KeyPath
                }
            };

            settings.Library.Endpoints.Add(endpoint.Id, endpoint);
            settings.Library.Save();

            AnsiConsole.MarkupLine($"Endpoint [wheat1]{endpoint.Id}[/] added");
            AnsiConsole.WriteLine(endpoint.ToJson());

            return 0;
        }
    }
}
