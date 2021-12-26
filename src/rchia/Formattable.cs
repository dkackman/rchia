using Newtonsoft.Json;
using System;

namespace rchia;

internal interface IFormattable
{
    object? GetValue();
}

/// <summary>
/// small helper to allow human readabke formatting of values for console output
/// Just serializes as a string in json
/// </summary>
[JsonConverter(typeof(JsonToStringConverter))]
internal sealed class Formattable<T> : IFormattable
{
    private readonly Func<T, string> _formatter;

    public Formattable(T value, string color)
        : this(value, v => $"[{color}]{v}[/]")
    {
    }

    public Formattable(T value, Func<T, string> formatter)
    {
        Value = value;
        _formatter = formatter;
    }

    public T Value { get; init; }

    public object? GetValue()
    {
        return Value;
    }

    public override string? ToString()
    {
        return _formatter(Value);
    }
}

internal sealed class JsonToStringConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is not IFormattable v)
        {
            writer.WriteNull();
        }
        else
        {
            writer.WriteValue(v.GetValue());
        }
    }
}
