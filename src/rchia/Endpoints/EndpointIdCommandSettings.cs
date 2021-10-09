using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace rchia.Endpoints
{
    public class EndpointIdCommandSettings : CommandSettings
    {
        [Description("The endpoint id to operate on.")]
        [CommandArgument(0, "<id>")]
        public string Id { get; init; } = string.Empty;

        [Description("Set output to verbose messages")]
        [CommandOption("-v|--verbose")]
        public bool Verbose { get; set; }

        public override ValidationResult Validate()
        {
            Library = EndpointLibrary.OpenLibrary();

            if (!Library.Endpoints.ContainsKey(Id))
            {
                return ValidationResult.Error($"No endpoint with an id of {Id} exists.");
            }

            return base.Validate();
        }

        public EndpointLibrary Library { get; protected set; } = new EndpointLibrary("");
    }
}
