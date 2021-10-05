using System;
using System.Collections.Generic;
using System.Linq;
using RefScout.Analyzer.Analyzers.Compatibility;
using RefScout.Analyzer.Config.Framework;

namespace RefScout.Analyzer.Tests;

internal static class AssHelp
{
    public static AssemblyRef Ref(
        Assembly from,
        Assembly to,
        string version = "1.0.0.0",
        ReferenceCompatibility compat = ReferenceCompatibility.Compatible,
        BindingRedirectStatus redirect = BindingRedirectStatus.Default) =>
        new(from, to, Version.Parse(version))
        {
            Compatibility = compat,
            BindingRedirectStatus = redirect
        };

    public static Assembly Ass(
        string name = "",
        string version = "1.0.0.0",
        PublicKeyToken? token = null,
        AssemblySource source = AssemblySource.Local,
        Version? originalVersion = null) =>
        new(new AssemblyIdentity(name, AssemblyIdentity.CultureNeutral, token ?? PublicKeyToken.Empty,
            Version.Parse(version)), source)
        {
            OriginalVersion = originalVersion
        };

    public static AssemblyIdentity Iden(
        string name = "Identity",
        string version = "1.0.0.0",
        string culture = "neutral",
        PublicKeyToken? token = null) =>
        new(name, culture, token ?? PublicKeyToken.Empty,
            Version.Parse(version));

    public static BindingIdentity BindIden(
        string name = "Identity",
        string culture = "neutral",
        PublicKeyToken? token = null) =>
        new(name, culture, token ?? PublicKeyToken.Empty);

    public static Assembly Build(
        string name,
        string version,
        AssemblySource source,
        IEnumerable<(string, string)>? references = null,
        bool isSystem = false,
        bool localSystem = false)
    {
        references ??= Enumerable.Empty<(string, string)>();

        return new Assembly(
            new AssemblyIdentity(name, AssemblyIdentity.CultureNeutral, PublicKeyToken.Empty, new Version(version)),
            "",
            source)
        {
            IsSystem = isSystem,
            IsLocalSystem = localSystem,
            RawReferences =
                references.Select(r =>
                    new AssemblyIdentity(r.Item1, AssemblyIdentity.CultureNeutral, PublicKeyToken.Empty,
                        new Version(r.Item2))).ToList()
        };
    }
}