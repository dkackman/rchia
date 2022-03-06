using rchia.Commands;

namespace rchia.Cats;

[Command("cats", Description = "Manage CATs, trades, and offers.")]
internal sealed class CatsCommands
{
    [Command("add-token", Description = "Add/Rename a CAT to the wallet by its asset ID")]
    public AddTokenCommand AddToken { get; init; } = new();

    [Command("create-wallet", Description = "Create a new CAT wallet")]
    public CreateWalletCommand Create { get; init; } = new();

    [Command("get-name", Description = "Get the name of a CAT wallet")]
    public GetNameCommand GetName { get; init; } = new();

    [Command("set-name", Description = "Set the name of a CAT wallet")]
    public SetNameCommand SetName { get; init; } = new();

    [Command("get-offers", Description = "Get the status of existing offers. Displays only active/pending offers by default.")]
    public GetOffersCommand GetOffers { get; init; } = new();

    [Command("get-offer", Description = "Get the details of a sepcific offer.")]
    public GetOffersCommand GetOffer { get; init; } = new();
}
