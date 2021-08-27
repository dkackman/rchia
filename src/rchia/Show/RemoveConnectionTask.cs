using System;
using System.Threading;
using System.Threading.Tasks;

using chia.dotnet;

namespace rchia.Show
{
    internal static class RemoveConnectionTask
    {
        public static async Task Run(FullNodeProxy fullNode, string nodeId, bool verbose)
        {
            if (verbose)
            {
                Console.WriteLine($"Removing {nodeId}...");
            }

            using var cts = new CancellationTokenSource(5000);
            await fullNode.CloseConnection(nodeId, cts.Token);

            if (verbose)
            {
                Console.WriteLine($"Removed {nodeId}.");
            }
        }
    }
}
