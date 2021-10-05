using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RefScout.Analyzer.Helpers;

[ExcludeFromCodeCoverage]
public class VersionJsonConverter : JsonConverter<Version>
{
    public override Version? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var str = reader.GetString();
        return str != null ? Version.Parse(str) : null;
    }

    public override void Write(
        Utf8JsonWriter writer,
        Version? version,
        JsonSerializerOptions options)
    {
        if (version == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(version.ToString());
        }
    }
}