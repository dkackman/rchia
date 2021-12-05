using rchia.Commands;

namespace rchia.Bech32;

internal sealed class HashFromAddressCommand : Command
{
    [Argument(0, Name = "address", Description = "The address to convert")]
    public string Address { get; init; } = string.Empty;

    [CommandTarget]
    public int Run()
    {
        return DoWork("Calculating hash...", output =>
        {
            output.WriteOutput("hash", Bech32M.AddressToPuzzleHash(Address).ToString(), Verbose);
        });
    }
}
