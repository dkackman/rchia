﻿using System;
using System.Threading;
using System.Threading.Tasks;

using chia.dotnet;

namespace rchia.Show
{
    internal static class ConnectionsTask
    {
        public static async Task Run(FullNodeProxy fullNode, bool verbose)
        {
            using var cts = new CancellationTokenSource(5000);
            var connections = await fullNode.GetConnections(cts.Token);

            Console.WriteLine("Connections:");
            var padding = verbose ? "                                                             " : "        ";
            Console.WriteLine($"Type      IP                   Ports       NodeID{padding}Last Connect       MiB Up|Dwn");

            foreach (var c in connections)
            {
                Console.Write($"{c.Type,-9} {c.PeerHost,-15}      {c.PeerPort,5}/{c.PeerServerPort,-5} ");

                var id = verbose ? c.NodeId : c.NodeId.Substring(2, 10) + "...";
                Console.Write($"{id} ");
                Console.Write($"{c.LastMessageDateTime.ToLocalTime():MMM dd HH:mm:ss}   ");

                var down = c.BytesRead / (double)(1024 * 1024);
                var up = c.BytesWritten / (double)(1024 * 1024);
                Console.Write($"{up,7:N1}|{down,-7:N1}");

                if (c.Type == NodeType.FULL_NODE)
                {
                    Console.WriteLine("");
                    var height = c.PeakHeight.HasValue ? c.PeakHeight : 0;
                    var hash = string.IsNullOrEmpty(c.PeakHash) ? "no info" :
                        verbose ? c.PeakHash : c.PeakHash.Substring(2, 10) + "...";

                    Console.Write("                               ");
                    Console.Write($"-SB Height: {height,8}    -Hash: {hash}");
                }
                Console.WriteLine("");
            }
        }
    }
}
