using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;

using chia.dotnet.console;

namespace rchia
{
    internal static class CommandLineBuilderExtensions
    {
        public static CommandLineBuilder UseAttributes(this CommandLineBuilder builder)
        {
            var root = new RootCommand();
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<CommandAttribute>() is not null);

            foreach (var t in types)
            {
                var verb = t.GetCustomAttribute<CommandAttribute>();
                var command = new Command(verb!.Name, verb.Description);

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

                    command.AddOption(option);
                }

                foreach (var (p, v) in t.GetAttributedProperties<ArgumentAttribute>().OrderBy(tuple => tuple.Attribute.Index))
                {
                    var argument = new Argument(v.Name)
                    {
                        ArgumentType = p.PropertyType
                    };

                    command.AddArgument(argument);
                }

                command.Handler = CommandHandler.Create(t.GetMethod("Run", BindingFlags.Public | BindingFlags.Instance)!);
                root.AddCommand(command);
            }

            return builder.AddCommand(root);
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
