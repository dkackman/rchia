using System;
using System.Threading.Tasks;

using chia.dotnet;
using chia.dotnet.console;

using CommandLine;

namespace rchia.Start
{ 
    [Verb("start", HelpText = "Start service groups.")]
    internal sealed class StartVerb
    {
        [Value(0, MetaName = "service-group", HelpText = "[all|node|harvester|farmer|farmer-no-wallet|farmer-only|timelord|timelord-only|timelord-launcher-only|wallet|wallet-only|introducer|simulator]")]
        public string? ServiceGroup { get; set; }

        [Option('r', "restart", HelpText = "Restart the specified service(s)")]
        public bool Restart { get; set; }
    }
}
