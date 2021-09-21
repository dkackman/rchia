using System;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;

namespace rchia.StartStop
{
    [Command("stop", Description = "Stop service groups.\nRequires a daemon endpoint.")]
    internal sealed class StopCommand : EndpointOptions
    {
        [Argument(0, Name = "service-group", Description = "[all|node|harvester|farmer|farmer-no-wallet|farmer-only|timelord|\ntimelord-only|timelord-launcher-only|wallet|wallet-only|introducer|simulator]")]
        public string? ServiceGroup { get; init; }

        [Option("d", "daemon", Description = "Stop the daemon service as well\nThe daemon cannot be restarted remotely")]
        public bool Daemon { get; init; }

        [Option("f", "force", Default = false, Description = "If specified in conjunstion with '-d', shut down the daemon without prompting")]
        public bool Force { get; init; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                if (ServiceGroup is null || !ServiceGroups.Groups.ContainsKey(ServiceGroup))
                {
                    throw new InvalidOperationException($"Unrecognized service group {ServiceGroup}. It must be one of\n  {string.Join("|", ServiceGroups.Groups.Keys)}.");
                }

                if (Daemon)
                {
                    if (!Confirm("The daemon cannot be restared remotely. You will need shell access to the node in order to restart it.\nAre you sure you want to stop the daemon?", Force))
                    {
                        throw new Exception("No services were stopped.");
                    }
                }

                using var tasks = await CreateTasksWithDaemon<StartStopTasks>(ServiceNames.Daemon);

                await DoWork("Stopping services...", async ctx => { await tasks.Stop(ServiceGroup); });
                if (Daemon)
                {
                    await DoWork("Stopping the daemon...", async ctx => { await tasks.StopDeamon(); });
                }
            });
        }
    }
}
