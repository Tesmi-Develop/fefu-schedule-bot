using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FefuScheduleBot.Utils;

public class JsonColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Excepted string for System.Drawing.Color.");

        var hex = reader.GetString() ?? string.Empty;
        if (string.IsNullOrEmpty(hex))
            return Color.Empty;
        
        if (hex.StartsWith("#"))
            hex = hex[1..];
        
        switch (hex.Length)
        {
            case 6:
            {
                var argb = Int32.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                return Color.FromArgb(255, Color.FromArgb(argb));
            }
            case 8:
            {
                var argb = Int32.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                return Color.FromArgb(argb);
            }
            default:
                throw new JsonException($"Bad format HEX: {hex}");
        }
    }
    
    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        var hex = $"#{value.R:X2}{value.G:X2}{value.B:X2}";
        writer.WriteStringValue(hex);
    }
}