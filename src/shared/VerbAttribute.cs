using System;

namespace chia.dotnet.console
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class VerbAttribute : Attribute
    {
        public VerbAttribute(string name, bool isDefault = false)
        {
            Name = name;
            IsDefault = isDefault;
        }

        public string Name { get; }

        public string? Description { get; set; }

        public bool IsDefault { get; }
    }
}
