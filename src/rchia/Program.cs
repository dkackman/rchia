using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
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
                .UseAttributes(Assembly.GetExecutingAssembly())
                .Build()
                .InvokeAsync(args);
        }
    }
}
