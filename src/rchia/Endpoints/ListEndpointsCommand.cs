using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace rchia.Endpoints
{
    [Description("Lists the ids of saved endpoint")]
    internal sealed class ListEndpointsCommand : Command<EndpointIdCommandSettings>
    {
        public override int Execute([NotNull] CommandContext context, [NotNull] EndpointIdCommandSettings settings)
        {
            var worker = new Worker()
            {
                Verbose = settings.Verbose
            };

            return worker.DoWork(() =>
            {
                var library = EndpointLibrary.OpenLibrary();

                foreach (var endpoint in library.Endpoints.Values)
                {
                    var isDefault = endpoint.IsDefault ? "[wheat1](default)[/]" : string.Empty;
                    AnsiConsole.MarkupLine($" - {endpoint.Id} {isDefault}");
                }

                AnsiConsole.MarkupLine($"[wheat1]{library.Endpoints.Count}[/] saved endpoint{(library.Endpoints.Count == 1 ? string.Empty : "s")}");
            });
        }
    }
}
