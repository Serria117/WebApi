using Newtonsoft.Json;

namespace WebApp.Mongo.DeserializedModel;

public class DecimalOrStringConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(decimal) || objectType == typeof(string);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            var stringValue = reader.Value?.ToString();
            if (stringValue != null && stringValue.EndsWith("%"))
            {
                // Remove the '%' sign and convert the percentage to a decimal.
                stringValue = stringValue.TrimEnd('%');
                if (decimal.TryParse(stringValue, out decimal percentageValue))
                {
                    return percentageValue / 100;
                }
            }
            return 0;  // Fallback if parsing fails
        }

        if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
        {
            return Convert.ToDecimal(reader.Value);
        }

        return 0;  // Fallback for unexpected token types
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is decimal decimalValue)
        {
            writer.WriteValue(decimalValue);
        }
        else
        {
            writer.WriteValue(value?.ToString());
        }
    }
}