using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace RefScout.Visualizers.Dgml;

internal static class XmlHelper
{
    public static string SerializeObjectUtf8<T>(T toSerialize)
    {
        _ = toSerialize ?? throw new ArgumentNullException(nameof(toSerialize));
        var serializer = new XmlSerializer(toSerialize.GetType());
        using var textWriter = new StringWriterUtf8();
        serializer.Serialize(textWriter, toSerialize);
        return textWriter.ToString();
    }

    private class StringWriterUtf8 : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}