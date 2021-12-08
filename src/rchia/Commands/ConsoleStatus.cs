using System;
using Spectre.Console;

namespace rchia.Commands;

public class ConsoleStatus : IStatus
{
    private readonly StatusContext _context;

    public ConsoleStatus(StatusContext? ctx)
    {
        _context = ctx ?? throw new ArgumentNullException(nameof(ctx));
    }

    public string Status { get => _context.Status; set { _context.Status = value; _context.Refresh(); } }
}
