using System.Threading.Tasks;

using chia.dotnet;

using rchia.Commands;

namespace rchia.Farm
{
    internal sealed class ChallengesCommand : EndpointOptions
    {
        [Argument(0, Name = "limit", Default = 20, Description = "Limit the number of challenges shown. Use 0 to disable the limit")]
        public int Limit { get; init; } = 20;

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                using var tasks = await CreateTasksWithDaemon<FarmTasks>(ServiceNames.Farmer);

                await DoWork("Retrieving challenges...", async ctx => { await tasks.Challenges(Limit); });
            });
        }
    }
}
