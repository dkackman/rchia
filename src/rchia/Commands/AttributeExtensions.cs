using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.Linq;
using System.Reflection;
using System.CommandLine.NamingConventionBinder;

namespace rchia.Commands;

public static class AttributeExtensions
{
    public static CommandLineBuilder UseAttributes(this CommandLineBuilder builder, Assembly assembly)
    {
        var commands = from type in assembly.GetTypes()
                       let attr = type.GetCustomAttribute<CommandAttribute>()
                       where attr is not null
                       orderby attr.Name
                       select CreateCommand(type, attr);

        foreach (var c in commands)
        {
            builder.Command.AddCommand(c);
        }

        return builder;
    }

    private static System.CommandLine.Command CreateCommand(Type type, CommandAttribute c)
    {
        var command = new System.CommandLine.Command(c.Name, c.Description);

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
                IsRequired = attr.IsRequired,
                IsHidden = attr.IsHidden
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
                ValueType = property.PropertyType,
            };

            if (attr.Default is not null)
            {
                argument.SetDefaultValue(attr.Default);
            }

            argument.Description = attr.Description;
            command.AddArgument(argument);
        }

        // and recurse to add subcommands
        foreach (var (property, subcommand) in type.GetAttributedProperties<CommandAttribute>().OrderBy(tuple => tuple.Attribute.Name))
        {
            command.AddCommand(CreateCommand(property.PropertyType, subcommand));
        }

        var target = type.GetCommandTarget(); // ?? throw new InvalidOperationException($"No method decorated with [CommandTarget] was found on {type.FullName}");
        if (target is not null)
        {
            command.Handler = CommandHandler.Create(target);
        }

        return command;
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
