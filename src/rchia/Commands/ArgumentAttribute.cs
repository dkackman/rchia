using System;

namespace rchia.Commands;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ArgumentAttribute : Attribute
{
    public ArgumentAttribute(int index)
    {
        Index = index;
    }

    public int Index { get; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public object? Default { get; init; }
}
