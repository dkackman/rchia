using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace rchia.Commands
{
    public abstract class BaseCommand : IConsoleMessage
    {
        [Option("v", "verbose", Description = "Set output to verbose messages")]
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

        public bool Confirm(string warning, string confirmation, bool force)
        {
            if (!force)
            {
                Console.WriteLine(warning);
                Console.Write(confirmation + " (y/n): ");
                var response = Console.ReadLine() ?? string.Empty;
                if (!response.ToLower().StartsWith('y'))
                {
                    Message("Cancelled");
                    return false;
                }
            }

            Message("Confirmed");
            return true;
        }

        public abstract Task<int> Run();
    }
}
