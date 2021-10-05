using System.Text.RegularExpressions;

namespace RefScout.Visualizers.Dot.Compiler;

internal static class DotHelpers
{
    private static readonly Regex ValidIdentifierPattern =
        new("^(?!#)([a-zA-Z\\200-\\377_][a-zA-Z\\200-\\3770-9_]*|[-]?(.[0-9]+|[0-9]+(.[0-9]+)?))$");

    public static string Quote(string value)
    {
        var formatted = FormatString(value);
        return ValidIdentifierPattern.IsMatch(value) ? formatted : "\"" + formatted + "\"";
    }

    private static string FormatString(string value) =>
        value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r\n", "\\n")
            .Replace("\n", "\\n");
}