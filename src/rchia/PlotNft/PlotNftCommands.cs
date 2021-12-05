using rchia.Commands;

namespace rchia.PlotNft
{
    [Command("plotnft", Description = "Manage your plot NFTs.\nRequires a wallet or daemon endpoint.")]
    internal sealed class PlotNftCommands
    {
        [Command("claim", Description = "Claim rewards from a plot NFT")]
        public ClaimNftCommand Claim { get; init; } = new();

        [Command("create", Description = "Create a plot NFT")]
        public CreatePlotNftCommand Create { get; init; } = new();

        [Command("get-login-link", Description = "Create a login link for a pool. To get the launcher id, use 'plotnft show'.")]
        public GetLoginLinkCommand GetLoginLink { get; init; } = new();

        [Command("inspect", Description = "Get Detailed plotnft information as JSON")]
        public InspectNftCommand Inspect { get; init; } = new();

        [Command("join", Description = "Join a plot NFT to a Pool")]
        public JoinPoolCommand Join { get; init; } = new();

        [Command("leave", Description = "Leave a pool and return to self-farming")]
        public LeavePoolCommand Leave { get; init; } = new();

        [Command("show", Description = "Show plotnft information")]
        public ShowPlotNftCommand Show { get; init; } = new();
    }
}
