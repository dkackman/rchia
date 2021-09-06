using rchia.Commands;

namespace rchia.Plots
{
    [Command("plots", Description = "Manage your plots.\nRequires a daemon endpoint.")]
    internal sealed class PlotsCommand
    {
        [Command("add", Description = "Adds a directory of plots")]
        public AddPlotsCommand Add { get; set; } = new();

        [Command("create", Description = "Create plots")]
        public CreatePlotsCommand Create { get; set; } = new();

        [Command("list", Description = "List plots on this node")]
        public ListPlotsCommand List { get; set; } = new();

        [Command("log", Description = "View the log for running plot jobs")]
        public LogPlotsCommand Log { get; set; } = new();

        [Command("queue", Description = "View the plot queue")]
        public PlotQueueCommand Queue { get; set; } = new();

        [Command("remove", Description = "Removes a directory of plots from config.yaml")]
        public RemovePlotsCommand Remove { get; set; } = new();

        [Command("show", Description = "Shows the directory of current plots")]
        public ShowPlotsCommand Show { get; set; } = new();
    }
}
