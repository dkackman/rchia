using rchia.Commands;

namespace rchia.Cats;

[Command("cats", Description = "Manage CATs, trades, and offers.")]
internal sealed class CatsCommands
{
    [Command("add-token", Description = "Add/Rename a CAT to the wallet by its asset ID")]
    public AddTokenCommand AddToken { get; init; } = new();

    [Command("create-wallet", Description = "Create a new CAT wallet")]
    public CreateWalletCommand CreateWallet { get; init; } = new();

    [Command("get-name", Description = "Get the name of a CAT wallet")]
    public GetNameCommand GetName { get; init; } = new();

    [Command("set-name", Description = "Set the name of a CAT wallet")]
    public SetNameCommand SetName { get; init; } = new();

    [Command("list", Description = "Get the status of existing offers. Displays only active/pending offers by default.")]
    public GetOffersCommand List { get; init; } = new();

    [Command("get", Description = "Get the details of an offer.")]
    public GetOfferCommand Get { get; init; } = new();

    [Command("cancel", Description = "Cancel an offer.")]
    public CancelOffer Cancel { get; init; } = new();

    [Command("take", Description = "Take an offer.")]
    public TakeOfferCommand Take { get; init; } = new();

    [Command("make", Description = "Make an offer.")]
    public MakeOfferCommand Create { get; init; } = new();

    [Command("check", Description = "Check the validity of an offer.")]
    public CheckOfferCommand Check { get; init; } = new();
}
