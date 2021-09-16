using System;
using System.Threading.Tasks;
using Spectre.Console;

namespace rchia.Commands
{
    internal interface IConsoleMessage
    {
        bool Verbose { get; set; }

        void Warning(string msg);

        void Helpful(string msg);

        void Message(string msg, bool important = false);

        void Message(Exception e);

        void NameValue(string name, object? value);

        bool Confirm(string warning, bool force);

        Task<T> Status<T>(string msg, Func<StatusContext, Task<T>> func);
    }
}
