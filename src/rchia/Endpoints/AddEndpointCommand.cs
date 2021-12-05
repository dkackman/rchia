using System;
using System.IO;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Endpoints;

internal sealed class AddEndpointCommand : Command
{
    [Argument(0, Name = "id", Description = "The id of the endpoint being added")]
    public string Id { get; init; } = string.Empty;

    [Argument(1, Name = "uri", Description = "The Uri of the endpoint being added")]
    public string Uri { get; init; } = string.Empty;

    [Argument(2, Name = "certpath", Description = "The full path to the .crt file to use for authentication")]
    public FileInfo? CertPath { get; init; }

    [Argument(3, Name = "keypath", Description = "The full path to the .key file to use for authentication")]
    public FileInfo? KeyPath { get; init; }

    [CommandTarget]
    public int Run()
    {
        return DoWork("Adding endpoint...", output =>
        {
            var library = EndpointLibrary.OpenLibrary();
            if (library.Endpoints.ContainsKey(Id))
            {
                throw new InvalidOperationException($"An endpoint with an id of {Id} already exists.");
            }

            if (CertPath is null || KeyPath is null)
            {
                throw new InvalidOperationException("Both cert-path and key-path must be specified.");
            }

            var endpoint = new Endpoint()
            {
                Id = Id,
                EndpointInfo = new EndpointInfo()
                {
                    Uri = new Uri(Uri),
                    CertPath = CertPath.FullName,
                    KeyPath = KeyPath.FullName
                }
            };

            library.Endpoints.Add(endpoint.Id, endpoint);
            library.Save();

            output.MarkupLine($"Endpoint [wheat1]{endpoint.Id}[/] added");
            output.WriteOutput(endpoint);
        });
    }
}
