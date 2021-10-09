using System.Threading.Tasks;
using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace rchia.Endpoints
{
    [Description("Tests the configuration of a saved endpoint")]
    internal sealed class TestEndpointCommand : AsyncCommand<TestEndpointCommand.TestEndpointSettings>
    {
        public class TestEndpointSettings : EndpointIdCommandSettings
        {
            [Description("Timeout in seconds")]
            [CommandOption("--to|--timeout")]
            [DefaultValue(30)]
            public int Timeout { get; init; } = 30;
        }

        public async override Task<int> ExecuteAsync(CommandContext context, TestEndpointCommand.TestEndpointSettings settings)
        {
            var worker = new Worker()
            {
                Verbose = settings.Verbose
            };

            return await worker.DoWorkAsync($"Testing endpoint {settings.Id}...", async ctx => 
            {
                var endpoint = settings.Library.Endpoints[settings.Id];

                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(ctx, endpoint.EndpointInfo, settings.Timeout * 1000);

                AnsiConsole.MarkupLine($"Successfully connected to [wheat1]{settings.Id}[/]");
            });
        }
    }
}
