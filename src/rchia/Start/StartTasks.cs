using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using chia.dotnet;
using chia.dotnet.console;

namespace rchia.Start
{
    internal sealed class StartTasks : ConsoleTask<DaemonProxy>
    {
        public StartTasks(DaemonProxy daemon, IConsoleMessage consoleMessage)
            : base(daemon, consoleMessage)
        {

        }
    }
}
