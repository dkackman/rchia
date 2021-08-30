using System;
using System.Threading.Tasks;

using chia.dotnet;
using chia.dotnet.console;

using CommandLine;

namespace rchia.Start
{ 
    [Verb("start", isDefault: true, HelpText = "Start services or service groups.")]
    internal sealed class StartVerb
    {
        [Option('r', "restart", HelpText = "Restart the specified service(s)")]
        public bool Restart { get; set; }
    }
}
