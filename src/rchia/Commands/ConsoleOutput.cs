using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

using Spectre.Console;

namespace rchia.Commands;

internal class ConsoleOutput : ICommandOutput
{
    private StatusContext? _statusContext;

    public bool Verbose { get; set; }

    public IStatus Status => new ConsoleStatus(_statusContext);

    public ICommandOutput SetContext(StatusContext? context)
    {
        _statusContext = context;
        return this;
    }

    public void WriteOutput(IDictionary<string, IEnumerable<IDictionary<string, string>>> output)
    {
        foreach (var table in output)
        {
            WriteTable(table.Value, table.Key);
        }
    }

    public void WriteOutput(string name, string value, bool verbose)
    {
        if (verbose)
        {
            MarkupLine($"[wheat1]{name.FromSnakeCase()}:[/] {value}");
        }
        else
        {
            WriteLine(value);
        }
    }

    public void WriteOutput(object output)
    {
        WriteLine(output.ToJson());
    }

    public void WriteOutput(IEnumerable<IDictionary<string, string>> output)
    {
        WriteTable(output, string.Empty);
    }

    public void WriteOutput(IEnumerable<string> output)
    {
        foreach (var value in output)
        {
            WriteLine(value);
        }
    }

    private static void WriteTable(IEnumerable<IDictionary<string, string>> output, string title)
    {
        var table = new Table
        {
            Title = new TableTitle($"[orange3]{title.FromSnakeCase()}[/]")
        };

        var first = output.FirstOrDefault();
        if (first != null)
        {
            // the first item holds the column names - it is assumed all items have the same keys
            foreach (var column in first.Keys)
            {
                table.AddColumn($"[orange3]{column.FromSnakeCase()}[/]");
            }

            // now add the values from all the rows
            foreach (var row in output)
            {
                table.AddRow(row.Values.ToArray());
            }
        }

        AnsiConsole.Write(table);
    }

    public void WriteOutput(IDictionary<string, string> output)
    {
        foreach (var value in output)
        {
            MarkupLine($"[wheat1]{value.Key.FromSnakeCase()}:[/] {value.Value}");
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

    public void WriteError(Exception e)
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
