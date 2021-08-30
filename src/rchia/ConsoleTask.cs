using chia.dotnet.console;

namespace rchia
{
    internal abstract class ConsoleTask
    {
        protected ConsoleTask(IConsoleMessage consoleMessage)
        {
            ConsoleMessage = consoleMessage;
        }

        public IConsoleMessage ConsoleMessage { get; init; }
    }
}
