using System;

namespace rchia.Commands
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class OptionAttribute : BaseAttribute
    {
        public OptionAttribute(string longName)
        {
            LongName = longName;
        }

        public OptionAttribute(char shortName)
        {
            ShortName = shortName.ToString();
        }


        public OptionAttribute(char shortName, string longName)
        {
            ShortName = shortName.ToString();
            LongName = longName;
        }


        public string? LongName { get; }

        public string? ShortName { get; }
    }
}
