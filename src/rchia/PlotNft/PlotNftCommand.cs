using rchia.Commands;

namespace rchia.PlotNft
{
    [Command("plotnft", Description = "Manage your plot NFTs.\nRequires a wallet or daemon endpoint.")]
    internal sealed class PlotNftCommand
    {
        [Command("claim", Description = "Claim rewards from a plot NFT")]
        public JoinPoolCommand Claim { get; set; } = new();

        [Command("create", Description = "Create a plot NFT")]
        public CreatePlotNftCommand Create { get; set; } = new();

        [Command("get-login-link", Description = "Create a login link for a pool. To get the launcher id, use 'plotnft show'.")]
        public GetLoginLinkCommand GetLoginLink { get; set; } = new();

        [Command("inspect", Description = "Get Detailed plotnft information as JSON")]
        public InspectNftCommand Inspect { get; set; } = new();

        [Command("join", Description = "Join a plot NFT to a Pool")]
        public JoinPoolCommand Join { get; set; } = new();

        [Command("leave", Description = "Leave a pool and return to self-farming")]
        public LeavePoolCommand Leave { get; set; } = new();

        [Command("show", Description = "Show plotnft information")]
        public ShowPlotNftCommand Show { get; set; } = new();
    }
}
