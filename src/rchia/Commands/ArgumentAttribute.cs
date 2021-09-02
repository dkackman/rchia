using System;

namespace rchia.Commands
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ArgumentAttribute : BaseAttribute
    {
        private string name;

        public ArgumentAttribute(int index)
        {
            Index = index;
            name = string.Empty;
        }

        public int Index { get; }

        public string Name
        {
            get => name;
            set => name = value ?? throw new ArgumentNullException(nameof(value));

        }
    }
}
