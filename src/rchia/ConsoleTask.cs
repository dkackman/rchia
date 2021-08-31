using chia.dotnet;
using chia.dotnet.console;

namespace rchia
{
    internal abstract class ConsoleTask<T> where T : ServiceProxy
    {
        protected ConsoleTask(T service, IConsoleMessage consoleMessage)
        {
            Service = service;
            ConsoleMessage = consoleMessage;
        }

        public IConsoleMessage ConsoleMessage { get; init; }

        public T Service { get; init; }
    }
}
