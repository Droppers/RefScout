using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Mono.Cecil;

namespace RefScout.Analyzer.Readers.Cecil;

internal static class GeneratedNameHelper
{
    private static int IndexOfBalancedParenthesis(string str, int openingOffset, char closing)
    {
        var opening = str[0];
        var depth = 1;
        for (var i = openingOffset + 1; i < str.Length; i++)
        {
            var c = str[i];
            if (c == opening)
            {
                depth++;
            }
            else if (c == closing)
            {
                depth--;
                if (depth == 0)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    public static bool IsCSharpGeneratedName(string name)
    {
        const string csPrefix = "CS$<";
        const char closingCharacter = '>';
        const string openingCharacter = "<";

        if (name.StartsWith(csPrefix, StringComparison.Ordinal))
        {
            return true;
        }

        var openBracketOffset = -1;
        if (name.StartsWith(openingCharacter, StringComparison.Ordinal))
        {
            openBracketOffset = 0;
        }

        if (openBracketOffset < 0)
        {
            return false;
        }

        var closeBracketOffset = IndexOfBalancedParenthesis(name, openBracketOffset, closingCharacter);
        if (closeBracketOffset < 0 || closeBracketOffset + 1 >= name.Length)
        {
            return false;
        }

        int c = name[closeBracketOffset + 1];
        return c is (>= '1' and <= '9') or (>= 'a' and <= 'z');
    }

    //System.EventHandler`1<SharpVectors.Runtime.SvgAlertArgs>
    public static bool IsVbBackingField(FieldDefinition field)
    {
        const string goodPrefix = "_";
        const string badPrefix = "__";

        if (field.Name.StartsWith(badPrefix, StringComparison.Ordinal) ||
            !field.Name.StartsWith(goodPrefix, StringComparison.Ordinal) ||
            !field.HasCustomAttributes)
        {
            return false;
        }

        return !field.FieldType.FullName.StartsWith(typeof(EventHandler).FullName!,
                   StringComparison.OrdinalIgnoreCase) &&
               field.CustomAttributes.Any(attribute =>
                   attribute.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName);
    }

    public static bool IsVbNetGeneratedName(string name)
    {
        const string vbPrefixOne = "VB$";
        const string vbPrefixTwo = "$VB$";
        const string closurePrefix = "_Closure$";

        return name.StartsWith(vbPrefixOne, StringComparison.Ordinal) ||
               name.StartsWith(vbPrefixTwo, StringComparison.Ordinal) ||
               name.StartsWith(closurePrefix, StringComparison.Ordinal);
    }

    public static bool IsCppCliNamespace(TypeDefinition type)
    {
        const string cppNamespace = "<CppImplementationDetails>";
        return type.Namespace.Equals(cppNamespace, StringComparison.Ordinal);
    }

    public static bool IsFSharpNamespace(TypeDefinition type)
    {
        const string generatedNamespacePrefix = "<StartupCode$";
        return type.Namespace.StartsWith(generatedNamespacePrefix, StringComparison.Ordinal);
    }
}