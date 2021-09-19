using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace rchia.Endpoints
{
    [Description("Removes a saved endpoint")]
    internal sealed class RemoveEndpointCommand : Command<EndpointIdCommandSettings>
    {
        public override int Execute([NotNull] CommandContext context, [NotNull] EndpointIdCommandSettings settings)
        {
            _ = settings.Library.Endpoints.Remove(settings.Id);
            settings.Library.Save();

            AnsiConsole.MarkupLine($"Endpoint [wheat1]{settings.Id}[/] removed");

            return 0;
        }
    }
}
