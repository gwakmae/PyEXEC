using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace PyExec.Converters
{
    // JsonConverter for GridLength
    public class GridLengthConverter : JsonConverter<GridLength>
    {
        public override GridLength Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (value == null) return new GridLength(1, GridUnitType.Star);

            // Use the built-in GridLengthConverter for string conversion
            return (GridLength)new System.Windows.GridLengthConverter().ConvertFromString(value)!;
        }

        public override void Write(Utf8JsonWriter writer, GridLength value, JsonSerializerOptions options)
        {
            // Use the built-in GridLengthConverter for string conversion
            writer.WriteStringValue(new System.Windows.GridLengthConverter().ConvertToString(value));
        }
    }
}