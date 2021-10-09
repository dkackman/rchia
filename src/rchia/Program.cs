using System;
using System.IO;
using System.Threading.Tasks;
using Spectre.Console.Cli;

using rchia.Endpoints;
using rchia.Farm;

namespace rchia
{
    internal static class Program
    {
        static Program()
        {
            ClientFactory.Initialize("rchia");
            ClientFactory2.Initialize("rchia");
        }

        private async static Task<int> Main(string[] args)
        {
            var app = new CommandApp();

            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            app.Configure(config =>
            {
                config.SetApplicationName("rchia");
                config.AddBranch("endpoints", endpoints =>
                {
                    endpoints.SetDescription("Manage saved endpoints.");
                    endpoints.AddCommand<AddEndpointCommand>("add")
                        .WithExample(new string[] { $"endpoints add local_daemon wss://localhost:55400 {Path.Combine(homeDir, "private_daemon.crt")} {Path.Combine(homeDir, "private_daemon.key")}" });
                    endpoints.AddCommand<ListEndpointsCommand>("list");
                    endpoints.AddCommand<RemoveEndpointCommand>("remove");
                    endpoints.AddCommand<ShowEndpointCommand>("show");
                    endpoints.AddCommand<SetDefaultEndpointCommand>("set-default");
                    endpoints.AddCommand<TestEndpointCommand>("test");
                });
                config.AddBranch("farm", farm =>
                {
                    farm.SetDescription("Manage your farm. Requires a daemon endpoint.");
                    farm.AddCommand<ChallengesCommand>("challenges")
                        .WithExample(new string[] { $"challenges --ep local_daemon" });
                    farm.AddCommand<SummaryCommand>("summary");
                });
            });

            return await app.RunAsync(args);
        }
    }
}
