using System;
using System.Diagnostics;
using System.Threading.Tasks;

using CommandLine;

namespace chia.dotnet.console
{
    public abstract class BaseVerb : IVerb
    {
        [Option('v', "verbose", HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
        
        public void Message(string msg, bool important = false)
        {
            if (Verbose || important)
            {
                Console.WriteLine(msg);
            }
            else
            {
                Debug.WriteLine(msg);
            }
        }

        public void Message(Exception? e)
        {
            if (e is not null)
            {
                Message(e.Message, true);
                if (Verbose) // if verbose - unwind the tree of exceptions
                {
                    Message(e.InnerException);
                }
            }
        }

        public abstract Task<int> Run();
    }
}
