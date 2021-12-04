using System;
using System.Collections.Generic;
using Spectre.Console;

namespace rchia.Commands
{
    public interface ICommandOuptut
    {
        bool Verbose { get; set; }

        bool IsInteractive { get; }
        IStatus CreateStatus(StatusContext? ctx);
        bool Confirm(string warning, bool force);
        void Helpful(string msg, bool important = false);
        void MarkupLine(string msg);
        void Message(Exception e);
        void Message(string msg, bool important = false);
        void NameValue(string name, object? value);
        void Warning(string msg);
        void WriteLine(string msg);
        void WriteOutput(IEnumerable<string> output);
    }
}