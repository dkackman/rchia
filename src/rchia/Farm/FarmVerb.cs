using System;
using System.Threading.Tasks;

using chia.dotnet;
using chia.dotnet.console;


namespace rchia.Farm
{
    [Verb("farm", Description = "Manage your farm.\nRequires a daemon endpoint.")]
    internal sealed class FarmVerb : SharedOptions
    {
        [Option('c', "challenges", Description = "Show the latest challenges")]
        public bool Challenges { get; set; }

        [Value(0, Name = "limit", Default = 20, Description = "Limit the number of challenges shown. Use 0 to disable the limit")]
        public int Limit { get; set; }

        [Option('s', "summary", Description = "Summary of farming information")]
        public bool Summary { get; set; }

        public override async Task<int> Run()
        {
            try
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Daemon);
                var daemon = new DaemonProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new FarmTasks(daemon, this);

                if (Challenges)
                {
                    await tasks.Challenges(Limit);
                }
                else if (Summary)
                {
                    await tasks.Summary();
                }
                else
                {
                    throw new InvalidOperationException("Unrecognized command");
                }

                return 0;
            }
            catch (Exception e)
            {
                Message(e);

                return -1;
            }
        }
    }
}
