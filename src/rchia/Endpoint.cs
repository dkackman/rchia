using chia.dotnet;

namespace rchia
{
    public record Endpoint
    {
        public string Id { get; init; } = string.Empty;

        public bool IsDefault { get; set; }

        public EndpointInfo EndpointInfo { get; init; } = new();
    }
}
