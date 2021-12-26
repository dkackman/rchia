using Newtonsoft.Json;
using System;

namespace rchia;

/// <summary>
/// small helper to allow colorization of values for console output
/// Just serializes as a string in json
/// </summary>
[JsonConverter(typeof(JsonToStringConverter))]
internal sealed class Formatable
{
    private readonly object? _value;
    private readonly string _color;

    public Formatable(object? value, string color)
    {
        _value = value;
        _color = color;
    }

    public string? Unformatted()
    {
        return _value?.ToString();
    }

    public override string? ToString()
    {
        return $"[{_color}]{_value}[/]";
    }

    private sealed class JsonToStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Formatable);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(((Formatable)value).Unformatted());
            }
        }
    }

}
