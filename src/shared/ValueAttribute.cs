using System;

namespace chia.dotnet.console
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ValueAttribute : BaseAttribute
    {
        private string name;

        public ValueAttribute(int index)
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
