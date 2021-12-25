using System;
using rchia.Commands;

namespace rchia.Bech32;

internal sealed class AddressFromHashCommand : Command
{
    [Argument(0, Name = "hash", Description = "The hash to convert")]
    public string Hash { get; init; } = string.Empty;

    [Option("p", "prefix", Default = "xch", ArgumentHelpName = "PREFIX", Description = "The coin prefix")]
    public string Prefix { get; init; } = "xch";

    [CommandTarget]
    public int Run()
    {
        return DoWork("Calculating address...", output =>
        {
            var bech = new Bech32M(Prefix);
            output.WriteOutput("address", bech.PuzzleHashToAddress(Hash), Verbose);
        });
    }
}
