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
            var types = from type in assembly.GetTypes()
                        let attr = type.GetCustomAttribute<CommandAttribute>()
                        where attr is not null
                        select (type, attr);

            foreach (var (type, attr) in types)
            {
                root.AddCommands(type, attr);
            }

            return builder;
        }

        private static void AddCommands(this Command parent, Type type, CommandAttribute c)
        {
            var command = new Command(c.Name, c.Description);

            // get all the options for the command
            foreach (var (property, attr) in type.GetAttributedProperties<OptionAttribute>())
            {
                var aliases = new List<string>();
                if (!string.IsNullOrEmpty(attr.ShortName))
                {
                    aliases.Add($"-{attr.ShortName}");
                }
                if (!string.IsNullOrEmpty(attr.LongName))
                {
                    aliases.Add($"--{attr.LongName}");
                }

                var option = new Option(aliases.ToArray(), attr.Description, property.PropertyType)
                {
                    ArgumentHelpName = attr.ArgumentHelpName ?? property.Name,
                    IsRequired = attr.IsRequired
                };
                if (attr.Default is not null)
                {
                    option.SetDefaultValue(attr.Default);
                }
                command.AddOption(option);
            }

            // add required arguments
            foreach (var (property, attr) in type.GetAttributedProperties<ArgumentAttribute>().OrderBy(tuple => tuple.Attribute.Index))
            {
                var argument = new Argument(attr.Name)
                {
                    ArgumentType = property.PropertyType,
                };
                if (attr.Default is not null)
                {
                    argument.SetDefaultValue(attr.Default);
                }
                command.AddArgument(argument);
            }

            // and recurse to add subcommands
            foreach (var (property, subcommand) in type.GetAttributedProperties<CommandAttribute>())
            {
                command.AddCommands(property.PropertyType, subcommand);
            }

            var target = type.GetCommandTarget() ?? throw new InvalidOperationException($"No method decorated with [CommandTarget] was found on {type.FullName}");

            command.Handler = CommandHandler.Create(target);
            parent.AddCommand(command);
        }

        public static MethodInfo? GetCommandTarget(this Type type)
        {
            var targets = from method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                          let attr = method.GetCustomAttribute<CommandTargetAttribute>(true)
                          where attr is not null
                          select method;

            return targets.FirstOrDefault();
        }

        private static IEnumerable<(PropertyInfo Propety, T Attribute)> GetAttributedProperties<T>(this Type type) where T : Attribute
        {
            return from property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   let attr = property.GetCustomAttribute<T>()
                   where attr is not null
                   select (property, attr);
        }
    }
}
