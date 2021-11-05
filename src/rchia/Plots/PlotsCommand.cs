using rchia.Commands;

namespace rchia.Plots
{
    [Command("plots", Description = "Manage your plots.\nRequires a harvester, plotter or daemon endpoint.")]
    internal sealed class PlotsCommand
    {
        [Command("add", Description = "Adds a directory of plots")]
        public AddPlotsCommand Add { get; init; } = new();

        [Command("create", Description = "Create plots")]
        public CreatePlotsCommand Create { get; init; } = new();

        [Command("list", Description = "List plots on this node")]
        public ListPlotsCommand List { get; init; } = new();

        [Command("log", Description = "View the log for running plot jobs or a specific plot")]
        public LogPlotsCommand Log { get; init; } = new();

        [Command("queue", Description = "View the plot queue")]
        public PlotQueueCommand Queue { get; init; } = new();

        [Command("refresh", Description = "Refreshes the harvester's plot list")]
        public RefreshPlotsCommand Refresh { get; init; } = new();

        [Command("remove", Description = "Removes a directory of plots from config.yaml")]
        public RemovePlotsCommand Remove { get; init; } = new();

        [Command("show", Description = "Shows the directory of current plots")]
        public ShowPlotsCommand Show { get; init; } = new();

        [Command("plotters", Description = "Lists installed and installable plotters")]
        public ListPlottersCommand Plotters { get; init; } = new();
    }
}
