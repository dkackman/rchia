using System;
using chia.dotnet;
using rchia.Commands;

namespace rchia
{
    public abstract class ConsoleTask<T> : IDisposable
        where T : ServiceProxy
    {
        protected ConsoleTask(T service, IConsoleMessage consoleMessage, int timeoutSeconds = 30)
        {
            Service = service;
            ConsoleMessage = consoleMessage;
            TimeoutMilliseconds = timeoutSeconds * 1000;
        }

        public int TimeoutMilliseconds { get; init; }

        public IConsoleMessage ConsoleMessage { get; init; }

        public T Service { get; init; }

        public void Dispose()
        {
            Service.RpcClient.Dispose();
        }
    }
}
