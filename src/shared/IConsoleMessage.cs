using System;

namespace chia.dotnet.console
{
    internal interface IConsoleMessage
    {
        bool Verbose { get; set; }

        void Message(string msg, bool important = false);

        void Message(Exception? e);
    }
}
