using System;
using System.Diagnostics;
using System.Collections.Generic;

using Spectre.Console;

namespace rchia.Commands
{
    internal class ConsoleOutput : ICommandOuptut
    {
        public bool Verbose { get; set; }

        public bool IsInteractive => true;

        public IStatus CreateStatus(StatusContext? ctx)
        {
            return new ConsoleStatus(ctx);
        }

        public void WriteOutput(IEnumerable<string> output)
        {
            foreach (var value in output)
            {
                WriteLine(value);
            }
        }

        public void MarkupLine(string msg)
        {
            AnsiConsole.MarkupLine(msg);
        }

        public void WriteLine(string msg)
        {
            AnsiConsole.WriteLine(msg);
        }

        public void Message(string msg, bool important = false)
        {
            if (important)
            {
                MarkupLine($"[yellow]{msg}[/]");
            }
            else if (Verbose)
            {
                MarkupLine(msg);
            }

            Debug.WriteLine(msg);
        }

        public void NameValue(string name, object? value)
        {
            MarkupLine($"[wheat1]{name}:[/] {value}");
        }

        public void Helpful(string msg, bool important = false)
        {
            if (Verbose || important)
            {
                MarkupLine($"[{(important ? "lime" : "grey")}]{msg}[/]");
            }
        }

        public void Warning(string msg)
        {
            MarkupLine($"[yellow]{msg}[/]");
        }

        public void Message(Exception e)
        {
            if (Verbose)
            {
                AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks);
            }
            else
            {
                MarkupLine($"[red]{e.Message}[/]");
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
    }
}
