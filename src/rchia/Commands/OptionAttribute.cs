using System;

namespace rchia.Commands;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class OptionAttribute : Attribute
{
    public OptionAttribute(string? shortName, string? longName = null)
    {
        if (shortName is null && longName is null)
        {
            throw new InvalidOperationException($"Both {nameof(shortName)} and {nameof(longName)} cannot be null");
        }

        ShortName = shortName;
        LongName = longName;
    }

    public string? LongName { get; init; }

    public string? ShortName { get; init; }

    public string? Description { get; init; }

    public string? ArgumentHelpName { get; init; }

    public bool IsRequired { get; init; }

    public bool IsHidden { get; init; }

    public object? Default { get; init; }
}
