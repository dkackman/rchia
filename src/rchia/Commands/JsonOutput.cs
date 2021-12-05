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

    public void WriteOutput(IDictionary<string, IEnumerable<IDictionary<string, string>>> output)
    {
        AnsiConsole.WriteLine(output.ToJson());
    }

    public void WriteOutput(string name, string value, bool verbose = false)
    {
        var output = new Dictionary<string, string>()
        {
            { name, value }
        };

        WriteOutput(output);
    }

    public void WriteOutput(object output)
    {
        AnsiConsole.WriteLine(output.ToJson());
    }

    public void WriteOutput(IEnumerable<IDictionary<string, string>> output)
    {
        AnsiConsole.WriteLine(output.SortAll().ToJson());
    }

    public void WriteOutput(IEnumerable<string> output)
    {
        AnsiConsole.WriteLine(output.ToJson());
    }

    public void WriteOutput(IDictionary<string, string> output)
    {
        AnsiConsole.WriteLine(output.Sort().ToJson());
    }

    public void MarkupLine(string msg)
    {
        Debug.WriteLine(msg);
    }

    public void WriteLine(string msg)
    {
        Debug.WriteLine(msg);
    }

    public void Message(string msg, bool important = false)
    {
        Debug.WriteLine(msg);
    }

    public void Helpful(string msg, bool important = false)
    {
        Debug.WriteLine(msg);
    }

    public void Warning(string msg)
    {
        Debug.WriteLine(msg);
    }

    public void WriteError(Exception e)
    {
        WriteOutput("error", e.Message);
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
