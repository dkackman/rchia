using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.CommandLine.Rendering;
using System.Reflection;
using System.Threading.Tasks;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia
{
    internal static class Program
    {
        static Program()
        {
            ClientFactory.Initialize("rchia");
        }

        private async static Task<int> Main(string[] args)
        {
            return await new CommandLineBuilder()
                .UseDefaults()
                .UseAnsiTerminalWhenAvailable()
                .UseAttributes(Assembly.GetExecutingAssembly())
                .Build()
                .InvokeAsync(args);
        }
    }
}
