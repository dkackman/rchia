using rchia.Commands;

namespace rchia.Farm
{
    [Command("farm", Description = "Manage your farm.\nRequires a daemon endpoint.")]
    internal sealed class FarmCommand
    {
        [Command("challenges", Description = "Show the latest challenges")]
        public ChallengesCommand Challenges { get; set; } = new();

        [Command("summary", Description = "Summary of farming information")]
        public SummaryCommand Summary { get; set; } = new();
    }
}
