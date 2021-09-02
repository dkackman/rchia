using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;

using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

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

        private static async Task<int> Main(string[] args)
        {
            return await BuildCommandLine()
                .UseDefaults()
                .Build()
                .InvokeAsync(args);
        }

        private static CommandLineBuilder BuildCommandLine()
        {
            var root = new RootCommand();
            var types = from t in Assembly.GetExecutingAssembly().GetTypes()
                        let a = t.GetCustomAttribute<CommandAttribute>()
                        where a is not null
                        select (t, a);

            foreach (var (t, c) in types)
            {
                root.AddCommands(t, c);
            }

            return new CommandLineBuilder(root);
        }

        private static void AddCommands(this Command baseCommand, Type t, CommandAttribute c)
        {
            var command = new Command(c.Name, c.Description);

            // get all the options for the command
            foreach (var (p, o) in t.GetAttributedProperties<OptionAttribute>())
            {
                var aliases = new List<string>();
                if (!string.IsNullOrEmpty(o.ShortName))
                {
                    aliases.Add($"-{o.ShortName}");
                }
                if (!string.IsNullOrEmpty(o.LongName))
                {
                    aliases.Add($"--{o.LongName}");
                }

                var option = new Option(aliases.ToArray(), o.Description, p.PropertyType)
                {
                    ArgumentHelpName = o.ArgumentHelpName ?? p.Name
                };
                if (o.Default is not null)
                {
                    option.SetDefaultValue(o.Default);
                }
                command.AddOption(option);
            }

            // add required arguments
            foreach (var (p, v) in t.GetAttributedProperties<ArgumentAttribute>().OrderBy(tuple => tuple.Attribute.Index))
            {
                var argument = new Argument(v.Name)
                {
                    ArgumentType = p.PropertyType,
                };
                if (v.Default is not null)
                {
                    argument.SetDefaultValue(v.Default);
                }
                command.AddArgument(argument);
            }

            // and recurse to add subcommands
            foreach (var (property, subcommand) in t.GetAttributedProperties<CommandAttribute>())
            {
                command.AddCommands(property.PropertyType, subcommand);
            }

            command.Handler = CommandHandler.Create(t.GetMethod("Run", BindingFlags.Public | BindingFlags.Instance)!);
            baseCommand.AddCommand(command);
        }

        private static IEnumerable<(PropertyInfo Propety, T Attribute)> GetAttributedProperties<T>(this Type t) where T : Attribute
        {
            return from p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   let a = p.GetCustomAttribute<T>()
                   where a is not null
                   select (p, a);
        }
    }
}
