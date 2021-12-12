using System;
using Spectre.Console;

namespace rchia.Commands;

public class ConsoleStatus : IStatus
{
    private readonly StatusContext? _context;

    public ConsoleStatus()
    {
    }

    public ConsoleStatus(StatusContext? ctx)
    {
        _context = ctx;
    }

    public string Status {
        get {
            if (_context is not null)
            {
                return _context.Status;
            }
            return string.Empty;
        }
        set {
            if (_context is not null)
            {
                _context.Status = value;
                _context.Refresh();
            }
        }
    }
}
