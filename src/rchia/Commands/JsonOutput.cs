using System;
using System.Diagnostics;
using System.Collections.Generic;

using Spectre.Console;

namespace rchia.Commands
{
    internal class JsonOutput : ICommandOuptut
    {
        public bool Verbose { get; set; }

        public bool IsInteractive => false;

        public IStatus CreateStatus(StatusContext? ctx)
        {
            return new NullStatus();
        }

        public void WriteOutput(IEnumerable<string> output)
        {
            AnsiConsole.WriteLine(output.ToJson());
        }

        public void MarkupLine(string msg)
        {
        }

        public void WriteLine(string msg)
        {
        }

        public void Message(string msg, bool important = false)
        { 
            Debug.WriteLine(msg);
        }

        public void NameValue(string name, object? value)
        {
        }

        public void Helpful(string msg, bool important = false)
        {
        }

        public void Warning(string msg)
        {
        }

        public void Message(Exception e)
        {
            Debug.Assert(false, "not sure what im gonna do here yet");
        }

        public bool Confirm(string warning, bool force)
        {
            if (!force)
            {
                throw new InvalidOperationException($"This operation requires confirmation and the force flag is not set. \n{warning}\n Aborting.");
            }

            return true;
        }
    }
}
