using System;

namespace rchia.Commands
{
    public interface IConsoleMessage
    {
        bool Verbose { get; init; }

        void MarkupLine(string msg);

        void WriteLine(string msg);

        void Warning(string msg);

        void Helpful(string msg, bool important = false);

        void Message(string msg, bool important = false);

        void Message(Exception e);

        void NameValue(string name, object? value);

        bool Confirm(string warning, bool force);
    }
}
