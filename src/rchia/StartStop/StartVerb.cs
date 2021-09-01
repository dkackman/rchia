using System;
using System.Linq;
using System.Threading.Tasks;

using chia.dotnet;
using chia.dotnet.console;

using CommandLine;

namespace rchia.StartStop
{ 
    [Verb("start", HelpText = "Start service groups.")]
    internal sealed class StartVerb : SharedOptions
    {
        [Value(0, MetaName = "service-group", HelpText = "[all|node|harvester|farmer|farmer-no-wallet|farmer-only|timelord|\ntimelord-only|timelord-launcher-only|wallet|wallet-only|introducer|simulator]")]
        public string? ServiceGroup { get; set; }

        [Option('r', "restart", HelpText = "Restart the specified service(s)")]
        public bool Restart { get; set; }

        private const string ServiceGroupNames = "all|node|harvester|farmer|farmer-no-wallet|farmer-only|timelord|timelord-only|timelord-launcher-only|wallet|wallet-only|introducer|simulator";
        private readonly string[] serviceGroups = ServiceGroupNames.Split("|");

        public override async Task<int> Run()
        {
            try
            {
                if (!serviceGroups.Any(s => s == ServiceGroup))
                {
                    throw new InvalidOperationException($"Unrecognized service group {ServiceGroup}. It must be one of\n  {ServiceGroupNames}.");
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
