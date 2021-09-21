using System;
using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;

namespace rchia.Show
{
    internal sealed class PruneCommand : EndpointOptions
    {
        [Option("a", "age", Default = 12, Description = "Prune nodes that haven't sent data in this number of hours")]
        public int Age { get; init; } = 12;

        [CommandTarget]
        public async override Task<int> Run()
        {
            if (Age < 1)
            {
                throw new InvalidOperationException("Age must be 1 or more");
            }

            return await Execute(async () =>
            {
                using var tasks = await CreateTasks<ShowTasks, FullNodeProxy>(ServiceNames.FullNode);

                await DoWork("Pruning stale connections...", async ctx => { await tasks.Prune(Age); });
            });
        }
    }
}
