using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

using Spectre.Console;

namespace rchia.Commands;

internal class ConsoleOutput : ICommandOutput
{
    private StatusContext? _statusContext;

    public bool Verbose { get; init; }

    public IStatus Status => new ConsoleStatus(_statusContext);

    public ICommandOutput SetContext(StatusContext? context)
    {
        _statusContext = context;
        return this;
    }

    public void WriteOutput(IDictionary<string, IEnumerable<IDictionary<string, object?>>> output)
    {
        foreach (var table in output)
        {
            WriteTable(table.Value, table.Key);
        }
    }

    public void WriteOutput(string name, object? value, bool verbose)
    {
        if (verbose)
        {
            WriteMarkupLine($"[wheat1]{name.FromSnakeCase()}:[/] {value}");
        }
        else
        {
            WriteLine($"{value}");
        }
    }

    public void WriteOutput(object output)
    {
        WriteLine(output.ToJson());
    }

    public void WriteOutput(IEnumerable<IDictionary<string, object?>> output)
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

    public void WriteOutput(IDictionary<string, object?> output)
    {
        foreach (var value in output)
        {
            WriteMarkupLine($"[wheat1]{value.Key.FromSnakeCase()}:[/] {Format(value.Value)}");
        }
    }

    private static object? Format(object? value)
    {
        if (value is null)
        {
            return null;
        }
        if (value.GetType() == typeof(ulong))
        {
            return ((ulong)value).ToString("N0");
        }
        if (value.GetType() == typeof(uint))
        {
            return ((uint)value).ToString("N0");
        }
        return value;
    }

    private static void WriteTable(IEnumerable<IDictionary<string, object?>> output, string title)
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
                table.AddRow(row.Values.Select(item => $"{Format(item)}").ToArray());
            }
        }

        AnsiConsole.Write(table);
    }

    public void WriteMarkupLine(string msg)
    {
        AnsiConsole.MarkupLine(msg);
    }

    public void WriteLine(string msg)
    {
        AnsiConsole.WriteLine(msg);
    }

    public void WriteMessage(string msg, bool important = false)
    {
        if (important)
        {
            WriteMarkupLine($"[yellow]{msg}[/]");
        }
        else if (Verbose)
        {
            WriteMarkupLine(msg);
        }

        Debug.WriteLine(msg);
    }

    public void WriteWarning(string msg)
    {
        WriteMarkupLine($"[yellow]{msg}[/]");
    }

    public void WriteError(Exception e)
    {
        if (Verbose)
        {
            AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks);
        }
        else
        {
            WriteMarkupLine($"[red]{e.Message}[/]");
        }
    }

    public bool Confirm(string warning, bool force)
    {
        if (!force)
        {
            if (!AnsiConsole.Confirm(warning, false))
            {
                WriteMessage("Cancelled");
                return false;
            }

            WriteMessage("Confirmed");
        }

        return true;
    }
}
