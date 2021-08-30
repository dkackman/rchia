using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using chia.dotnet;
using chia.dotnet.console;

namespace rchia.Start
{
    internal sealed class StartTasks : ConsoleTask
    {
        public StartTasks(FullNodeProxy fullNode, IConsoleMessage consoleMessage)
            : base(consoleMessage)
        {

        }
    }
}
