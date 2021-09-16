﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Spectre.Console;

namespace rchia.Commands
{
    public abstract class Command : IConsoleMessage
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

        public bool Confirm(string warning, bool force)
        {
            if (!force)
            {
                if (!AnsiConsole.Confirm(warning, false))
                {
                    Message("Cancelled");
                    return false;
                }
                Message("Confirmed");
            }

            return true;
        }

        public abstract Task<int> Run();

        protected async Task<int> Execute(Func<Task> run)
        {
            try
            {
                await run();

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occured: {(Verbose ? e.GetType().Name : string.Empty)}");
                Message(e);

                return -1;
            }
        }
    }
}
