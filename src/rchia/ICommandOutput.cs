using System;
using System.Collections.Generic;
using Spectre.Console;

namespace rchia;

public interface ICommandOutput
{
    bool Verbose { get; init; }

    ICommandOutput SetContext(StatusContext? context);

    IStatus Status { get; }

    bool Confirm(string warning, bool force);

    void WriteMarkupLine(string msg);
    void WriteMessage(string msg, bool important = false);
    void WriteWarning(string msg);
    void WriteLine(string msg);
    void WriteError(Exception e);

    void WriteOutput(string name, object? value, bool verbose);
    void WriteOutput(object output);
    void WriteOutput(IEnumerable<IDictionary<string, object?>> output);
    void WriteOutput(IEnumerable<string> output);
    void WriteOutput(IDictionary<string, object?> output);
    void WriteOutput(IDictionary<string, IEnumerable<IDictionary<string, object?>>> output);
}
