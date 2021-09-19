using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace rchia.Endpoints
{
    [Description("Sets the endpoint to be the default for --default-endpoint")]
    internal sealed class SetDefaultEndpointCommand : Command<EndpointIdCommandSettings>
    {
        public override int Execute([NotNull] CommandContext context, [NotNull] EndpointIdCommandSettings settings)
        {
            foreach (var endpoint in settings.Library.Endpoints.Values)
            {
                endpoint.IsDefault = endpoint.Id == settings.Id;
            }

            settings.Library.Save();
            AnsiConsole.MarkupLine($"Endpoint [wheat1]{settings.Id}[/] is now the default");

            return 0;
        }
    }
}
