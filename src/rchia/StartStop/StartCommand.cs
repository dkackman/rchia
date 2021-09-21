﻿using System;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;

namespace rchia.StartStop
{
    [Command("start", Description = "Start service groups.\nRequires a daemon endpoint.")]
    internal sealed class StartCommand : EndpointOptions
    {
        [Argument(0, Name = "service-group", Description = "[all|node|harvester|farmer|farmer-no-wallet|farmer-only|timelord|\ntimelord-only|timelord-launcher-only|wallet|wallet-only|introducer|simulator]")]
        public string? ServiceGroup { get; init; }

        [Option("r", "restart", Description = "Restart the specified service(s)")]
        public bool Restart { get; init; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var tasks = await CreateTasksWithDaemon<StartStopTasks>(ServiceNames.Daemon);

                if (ServiceGroup is null || !ServiceGroups.Groups.ContainsKey(ServiceGroup))
                {
                    throw new InvalidOperationException($"Unrecognized service group {ServiceGroup}. It must be one of\n  {string.Join('|', ServiceGroups.Groups.Keys)}.");
                }

                await DoWork("Starting services...", async ctx => { await tasks.Start(ServiceGroup, Restart); });
            });
        }
    }
}
