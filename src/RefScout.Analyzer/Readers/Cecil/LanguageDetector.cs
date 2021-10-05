using System;
using System.Collections.Generic;
using Mono.Cecil;

namespace RefScout.Analyzer.Readers.Cecil;

public static class LanguageDetector
{
    private static readonly Dictionary<string, AssemblySourceLanguage> AssemblyToSourceLanguageMapping = new()
    {
        { "FSharp.Core", AssemblySourceLanguage.FSharp },
        { "Mono.CSharp", AssemblySourceLanguage.CSharp },
        { "Microsoft.CSharp", AssemblySourceLanguage.CSharp },
        { "Microsoft.VisualBasic", AssemblySourceLanguage.VbNet }
    };

    public static AssemblySourceLanguage DetectLanguageFromAssembly(AssemblyDefinition definition)
    {
        _ = definition ?? throw new ArgumentNullException(nameof(definition));

        AssemblySourceLanguage language;
        foreach (var type in definition.MainModule.Types)
        {
            if (GeneratedNameHelper.IsCppCliNamespace(type))
            {
                return AssemblySourceLanguage.CppCli;
            }

            if (GeneratedNameHelper.IsFSharpNamespace(type))
            {
                return AssemblySourceLanguage.FSharp;
            }

            language = GetLanguageFromName(type.Name);
            if (language != AssemblySourceLanguage.Unknown)
            {
                return language;
            }
        }

        language = AnalyzeTypesDeep(definition.MainModule.Types);
        return language != AssemblySourceLanguage.Unknown
            ? language
            : DetermineFromReferences(definition.MainModule.AssemblyReferences);
    }

    private static AssemblySourceLanguage DetermineFromReferences(IEnumerable<AssemblyNameReference> references)
    {
        foreach (var reference in references)
        {
            if (AssemblyToSourceLanguageMapping.ContainsKey(reference.Name))
            {
                return AssemblyToSourceLanguageMapping[reference.Name];
            }
        }

        return AssemblySourceLanguage.Unknown;
    }

    private static AssemblySourceLanguage GetLanguageFromName(string name)
    {
        if (GeneratedNameHelper.IsCSharpGeneratedName(name))
        {
            return AssemblySourceLanguage.CSharp;
        }

        if (GeneratedNameHelper.IsVbNetGeneratedName(name))
        {
            return AssemblySourceLanguage.VbNet;
        }

        return AssemblySourceLanguage.Unknown;
    }

    private static AssemblySourceLanguage AnalyzeTypesDeep(IEnumerable<TypeDefinition> types)
    {
        foreach (var type in types)
        {
            var language = GetLanguageFromName(type.Name);
            if (language != AssemblySourceLanguage.Unknown)
            {
                return language;
            }

            foreach (var field in type.Fields)
            {
                language = GetLanguageFromName(field.Name);
                if (language != AssemblySourceLanguage.Unknown)
                {
                    return language;
                }

                if (GeneratedNameHelper.IsVbBackingField(field))
                {
                    return AssemblySourceLanguage.VbNet;
                }
            }

            foreach (var method in type.Methods)
            {
                language = GetLanguageFromName(method.Name);
                if (language != AssemblySourceLanguage.Unknown)
                {
                    return language;
                }
            }

            language = AnalyzeTypesDeep(type.NestedTypes);
            if (language != AssemblySourceLanguage.Unknown)
            {
                return language;
            }
        }

        return AssemblySourceLanguage.Unknown;
    }
}