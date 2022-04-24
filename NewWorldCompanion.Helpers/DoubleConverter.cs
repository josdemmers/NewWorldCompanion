﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NewWorldCompanion.Helpers
{
    public class DoubleConverter : JsonConverter<double>
    {
        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options) =>
            writer.WriteNumberValue(value);

        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType switch
            {
                JsonTokenType.True => 0.00,
                JsonTokenType.False => 0.00,
                JsonTokenType.String => double.TryParse(reader.GetString(), out var b) ? b : 0.00,
                JsonTokenType.Number => reader.TryGetDouble(out double d) ? d : 0.00,
                JsonTokenType.Null => 0.00,
                _ => throw new JsonException(),
            };
    }
}