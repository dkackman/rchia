using rchia.Commands;

namespace rchia.Bech32
{
    [Command("bech32", Description = "Convert addresses to and from puzzle hashes.")]
    internal sealed class Bech32Command
    {
        [Command("hash-from-address", Description = "Convert a puzzle hash to an addrees")]
        public HashFromAddressCommand HashFromAddress { get; init; } = new();

        [Command("address-from-hash", Description = "Convert an address to a puzzle hash")]
        public AddressFromHashCommand AddressFromHash { get; init; } = new();
    }
}
