using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace rchia.Endpoints
{
    [Description("Lists the ids of saved endpoint")]
    internal sealed class ListEndpointsCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            var library = EndpointLibrary.OpenLibrary();

            foreach (var endpoint in library.Endpoints.Values)
            {
                var isDefault = endpoint.IsDefault ? "[wheat1](default)[/]" : string.Empty;
                AnsiConsole.MarkupLine($" - {endpoint.Id} {isDefault}");
            }

            AnsiConsole.MarkupLine($"[wheat1]{library.Endpoints.Count}[/] saved endpoint{(library.Endpoints.Count == 1 ? string.Empty : "s")}");

            return 0;
        }
    }
}
