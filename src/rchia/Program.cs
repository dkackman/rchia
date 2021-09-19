using System.Threading.Tasks;
using rchia.Endpoints;
using Spectre.Console.Cli;

namespace rchia
{
    internal static class Program
    {
        static Program()
        {
            ClientFactory.Initialize("rchia");
        }

        private async static Task<int> Main(string[] args)
        {
            var app = new CommandApp();

            app.Configure(config =>
            {
                config.SetApplicationName("rchia");
                config.AddBranch("endpoints", endpoints =>
                {
                    endpoints.SetDescription("Manage saved endpoints.");
                    endpoints.AddCommand<AddEndpointCommand>("add")
                        .WithExample(new string[] { @"endpoints add local wss://localhost:55400 C:\path\to\private_daemon.crt C:\path\to\private_daemon.key" });
                    endpoints.AddCommand<ListEndpointsCommand>("list");
                    endpoints.AddCommand<RemoveEndpointCommand>("remove");
                    endpoints.AddCommand<ShowEndpointCommand>("show");
                    endpoints.AddCommand<SetDefaultEndpointCommand>("set-default");
                    endpoints.AddCommand<TestEndpointCommand>("test");
                });
            });

            return await app.RunAsync(args);
        }
    }
}
