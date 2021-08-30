using System;
using System.Threading.Tasks;

using chia.dotnet;
using chia.dotnet.console;

using CommandLine;

namespace rchia.Farm
{
    [Verb("farm", HelpText = "Manage your farm.\nRequires a farmer or a daemon endpoint.")]
    internal sealed class FarmVerb : SharedOptions
    {
        [Option('c', "challenges", HelpText = "Show the latest challenges")]
        public bool Challenges { get; set; }

        [Option('s', "summary", HelpText = "Summary of farming information")]
        public bool Summary { get; set; }


        public override async Task<int> Run()
        {
            try
            {
                using var rpcClient = await ClientFactory.Factory.CreateRpcClient(this, ServiceNames.Farmer);
                var farmer = new FarmerProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new FarmTasks(farmer, Verbose);

                if (Challenges)
                {
                    await tasks.Services();
                }
                else if (Summary)
                {
                    await tasks.Services();
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
