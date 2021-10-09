using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using Spectre.Console.Cli;

namespace rchia.Endpoints
{
    [Description("Shows the details of a saved endpoint")]
    internal sealed class ShowEndpointCommand : Command<EndpointIdCommandSettings>
    {
        public override int Execute([NotNull] CommandContext context, [NotNull] EndpointIdCommandSettings settings)
        {
            var worker = new Worker()
            {
                Verbose = settings.Verbose
            };

            return worker.DoWork(() =>
            {
                var endpoint = settings.Library.Endpoints[settings.Id];
                AnsiConsole.WriteLine(endpoint.ToJson());
            });
        }
    }
}
