using System;
using System.Threading;
using System.Threading.Tasks;

using chia.dotnet;

namespace rchia.Show
{
    internal static class AddConnectionTask
    {
        public static async Task Run(FullNodeProxy fullNode, string hostUri, bool verbose)
        {
            if (verbose)
            {
                Console.WriteLine($"Connecting to {hostUri}...");
            }

            using var cts = new CancellationTokenSource(5000);
            var uri = new Uri("https://" + hostUri); // need to add a scheme so uri can be parsed
            await fullNode.OpenConnection(uri.Host, uri.Port, cts.Token);

            if (verbose)
            {
                Console.WriteLine($"Successfully connected to {hostUri}.");
            }
        }
    }
}
