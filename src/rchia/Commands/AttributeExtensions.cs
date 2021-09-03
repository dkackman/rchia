using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace rchia.Commands
{
    public static class AttributeExtensions
    {
        public static CommandLineBuilder UseAttributes(this CommandLineBuilder builder, Assembly assembly)
        {
            var root = builder.Command;
            var types = from t in assembly.GetTypes()
                        let a = t.GetCustomAttribute<CommandAttribute>()
                        where a is not null
                        select (t, a);

            foreach (var (t, c) in types)
            {
                root.AddCommands(t, c);
            }

            return builder;
        }

        private static void AddCommands(this Command parent, Type t, CommandAttribute c)
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
                    ArgumentHelpName = o.ArgumentHelpName ?? p.Name,
                    IsRequired = o.IsRequired
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

            var targets = from m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                          let a = t.GetCustomAttributes<CommandTargetAttribute>(true)
                          where a is not null
                          select m;

            var target = targets.FirstOrDefault() ?? throw new InvalidOperationException($"No method decorated with the [CommandTarget] attribute was found on type {t.FullName}");
            command.Handler = CommandHandler.Create(target);
            parent.AddCommand(command);
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
