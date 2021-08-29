﻿using System.Linq;
using System.Reflection;
using System.Diagnostics;

using CommandLine;

using chia.dotnet.console;

namespace rchia
{
    internal static class Program
    {
        static Program()
        {
            ClientFactory.Initialize("rchia");
        }

        public static int Main(string[] args)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<VerbAttribute>() is not null).ToArray();

            _ = Parser.Default.ParseArguments(args, types)
                  .WithParsed(Runner.Run)
                  .WithNotParsed(Runner.HandleErrors);

            Debug.WriteLine($"Exit code {Runner.ReturnCode}");
            return Runner.ReturnCode;
        }
    }
}
