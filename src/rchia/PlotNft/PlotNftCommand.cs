using rchia.Commands;

namespace rchia.PlotNft
{
    [Command("plotnft", Description = "Manage your plot NFTs.\nRequires a daemon endpoint.")]
    internal sealed class PlotNftCommand
    {
        [Command("create", Description = "Create a plot NFT.")]
        public CreatePlotNftCommand Create { get; set; } = new();

        [Command("get-login-link", Description = "Create a login link for a pool. To get the launcher id, use 'plotnft show'.")]
        public GetLoginLinkCommand GetLoginLink { get; set; } = new();

        [Command("leave", Description = "Leave a pool and return to self-farming")]
        public LeavePoolCommand Leave { get; set; } = new();

        [Command("show", Description = "Show plotnft information")]
        public ShowPlotNftCommand Show { get; set; } = new();
    }
}
