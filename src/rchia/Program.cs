using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Text;

using CommandLine;

using chia.dotnet.console;

namespace rchia
{
    internal static class Program
    {
        internal static readonly ClientFactory Factory = new("rchia");

        static Program()
        {
            Initialize();
        }

        public static int Main(string[] args)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToArray();

            _ = Parser.Default.ParseArguments(args, types)
                  .WithParsed(Runner.Run)
                  .WithNotParsed(Runner.HandleErrors);

            Debug.WriteLine($"Exit code {Runner.ReturnCode}");
            return Runner.ReturnCode;
        }

        public static string SettingDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rchia");

        public static string SettingFilePath => Path.Combine(SettingDirectory, "config.json");

        internal static void Initialize()
        {
            try
            {
                // create the settings folder and file if they don't exist
                if (!Directory.Exists(SettingDirectory))
                {
                    _ = Directory.CreateDirectory(SettingDirectory);
                }

                if (!File.Exists(SettingFilePath))
                {
                    var defaultEndPointFile = Path.Combine(SettingDirectory, "endpoints.json");
                    var buffer = new StringBuilder()
                        .AppendLine("{")
                        .Append("  \"endpointfile\": ")
                        .AppendLine($"\"{defaultEndPointFile}\"")
                        .Append("}");

                    using var writer = new StreamWriter(SettingFilePath, false);
                    writer.WriteLine(buffer);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}
