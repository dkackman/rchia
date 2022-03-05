using rchia.Commands;

namespace rchia.Cats;

[Command("cats", Description = "Manage CATs, trades, and offers.")]
internal sealed class CatsCommands
{
    [Command("create-wallet", Description = "Create a new CAT wallet")]
    public CreateCatWalletCommand Create { get; init; } = new();

    [Command("get-name", Description = "Get the name of a CAT wallet")]
    public GetCatNameCommand GetName { get; init; } = new();

    [Command("set-name", Description = "Set the name of a CAT wallet")]
    public SetCatNameCommand SetName { get; init; } = new();
}
