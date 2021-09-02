using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using chia.dotnet;
using chia.dotnet.console;

namespace rchia.Keys
{
    internal class KeysTasks : ConsoleTask<WalletProxy>
    {
        public KeysTasks(WalletProxy wallet, IConsoleMessage consoleMessage)
            : base(wallet, consoleMessage)
        {
        }

        public async Task Add()
        {

        }

        public async Task Delete()
        {

        }

        public async Task DeleteAll()
        {

        }

        public async Task Generate()
        {

        }

        public async Task GenerateAndPrint()
        {

        }

        public async Task Show()
        {

        }

        public async Task Sign()
        {

        }

        public async Task Verify()
        {

        }
    }
}
