using System.ComponentModel;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;

namespace rchia.Endpoints
{
    [Description("Tests the configuration of a saved endpoint")]
    internal sealed class TestEndpointCommand : AsyncCommand<EndpointIdCommandSettings>
    {
        public async override Task<int> ExecuteAsync(CommandContext context, EndpointIdCommandSettings settings)
        {
            var endpoint = settings.Library.Endpoints[settings.Id];
            await ClientFactory.Factory.TestConnection(endpoint.EndpointInfo);

            AnsiConsole.MarkupLine($"Successfully connected to [wheat1]{settings.Id}[/]");

            return 0;
        }
    }
}
