using chia.dotnet;

namespace rchia.Endpoints
{
    public record Endpoint
    {
        public string Id { get; init; } = string.Empty;

        public EndpointInfo EndpointInfo { get; init; } = new();
    }
}
