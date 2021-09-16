using System;

namespace rchia.Commands
{
    internal interface IConsoleMessage
    {
        bool Verbose { get; set; }

        void Message(string msg, bool important = false);

        void Message(Exception? e);

        bool Confirm(string warning, bool force);
    }
}
