using System;

namespace RefScout.Analyzer;

public record AssemblyIdentity(string Name, string Culture, PublicKeyToken PublicKeyToken, Version Version)
{
    public const string CultureNeutral = "neutral";

    private static readonly Version ZeroVersion = new(0, 0);

    public AssemblyIdentity(string name, string culture, PublicKeyToken publicKeyToken) : this(name, culture,
        publicKeyToken, ZeroVersion) { }

    public AssemblyIdentity(string name) : this(name, CultureNeutral, PublicKeyToken.Empty, ZeroVersion) { }

    public bool IsWindowsRuntime { get; init; }

    public virtual string Id => $"{Name},{Version},{Culture},{PublicKeyToken}";

    public virtual string FullName =>
        Version != ZeroVersion
            ? $"{Name}, Version={Version}, Culture={Culture}, PublicKeyToken={PublicKeyToken}"
            : $"{Name}, Culture={Culture}, PublicKeyToken={PublicKeyToken}";

    public bool IsStrongNamed => PublicKeyToken != PublicKeyToken.Empty;
}