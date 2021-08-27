using System;
using System.Threading.Tasks;

using chia.dotnet;
using chia.dotnet.console;

using rchia.Endpoints;

using CommandLine;

namespace rchia
{
    [Verb("endpoint", HelpText = "Manage a list of saved endpoints")]
    internal sealed class EndpointVerb : BaseVerb
    {
        [Option('l', "list", HelpText = "Show the list of saved endpoints")]
        public bool List { get; set; }

        [Option('a', "add", HelpText = "[ID] [URI] [CRT PATH] [KEY PATH] Adds an endpoint to list")]
        public string Add { get; set; }

        [Option('r', "remove", HelpText = "[ID] Removes an endpoint from list")]
        public string Remove { get; set; }

        [Option('s', "show", HelpText = "[ID] Shows the details of a saved endpoint")]
        public string Show { get; set; }

        [Option('t', "test", HelpText = "[ID] Test the connection to a saved endpoint")]
        public string Test { get; set; }

        public override async Task<int> Run()
        {
            try
            {

                await Task.CompletedTask;
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
