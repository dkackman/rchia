﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using rchia.Commands;

namespace rchia.Endpoints
{
    internal sealed class ShowEndpointCommand : Command
    {
        [Argument(0, Name = "id", Description = "The id of the endpoint to show")]
        public string Id { get; set; } = string.Empty;

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                var library = EndpointsCommand.OpenLibrary();

                if (!library.Endpoints.ContainsKey(Id))
                {
                    throw new InvalidOperationException($"There is no saved endpoint with an id of {Id}.");
                }

                var endpoint = library.Endpoints[Id];
                WriteLine(endpoint.ToJson());

                await Task.CompletedTask;
            });
        }
    }
}
