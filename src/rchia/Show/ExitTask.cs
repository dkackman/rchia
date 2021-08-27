using System;
using System.Threading;
using System.Threading.Tasks;

using chia.dotnet;

namespace rchia.Show
{
    internal static class ExitTask
    {
        public static async Task Run(FullNodeProxy fullNode, bool verbose)
        {
            if (verbose)
            {
                Console.WriteLine("Stopping the full node...");
            }

            using var cts = new CancellationTokenSource(5000);
            await fullNode.StopNode(cts.Token);
        }
    }
}
