using System;
using System.Diagnostics;
using System.Collections.Generic;

using Spectre.Console;

namespace rchia.Commands;

internal class JsonOutput : ICommandOutput
{
    public bool Verbose { get; set; }

    public IStatus Status => new NullStatus();

    public ICommandOutput SetContext(StatusContext? context)
    {
        return this;
    }

    public void WriteOutput(object output)
    {
        WriteLine(output.ToJson());
    }

    public void WriteOutput(IEnumerable<IDictionary<string, string>> output)
    {
        AnsiConsole.WriteLine(output.ToJson());
    }

    public void WriteOutput(IEnumerable<string> output)
    {
        AnsiConsole.WriteLine(output.ToJson());
    }

    public void WriteOutput(IDictionary<string, string> output)
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

    public void Helpful(string msg, bool important = false)
    {
    }

    public void Warning(string msg)
    {
    }

    public void Message(Exception e)
    {
        WriteOutput(new Dictionary<string, string>()
                    {
                        { "error", e.Message }
                    }
        );
    }

    public bool Confirm(string warning, bool force)
    {
        if (!force)
        {
            throw new InvalidOperationException($"This operation requires confirmation and the force flag is not set.\n\t{warning}\nAborting.");
        }

        return true;
    }
}
