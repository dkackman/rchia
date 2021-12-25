using rchia.Commands;

namespace rchia.Blocks;

[Command("blocks", Description = "Show informations about blocks and coins.\nRequires a daemon or full_node endpoint.")]
internal sealed class BlocksCommands
{
    [Command("block", Description = "Look up a block by height or hash")]
    public BlockHeaderCommand BlockHeader { get; init; } = new();

    [Command("adds-and-removes", Description = "Shows additions and removals by block height or hash")]
    public AdditionsAndRemovalsCommand AddsAndRemoves { get; init; } = new();

    [Command("recent", Description = "Show the most recent blocks")]
    public RecentBlocksCommand RecentBlocks { get; init; } = new();
}
