using System;

namespace rchia.Commands
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class OptionAttribute : Attribute
    {
        public OptionAttribute(string? shortName, string? longName = null)
        {
            ShortName = shortName;
            LongName = longName;
        }

        public string? LongName { get; }

        public string? ShortName { get; }

        public string? Description { get; set; }

        public string? ArgumentHelpName { get; set; }

        public bool IsRequired { get; set; }

        public object? Default { get; set; }
    }
}
