using System;

namespace RefScout.Analyzer.Analyzers.Compatibility;

public interface IVersionComparer
{
    ReferenceCompatibility Compare(Version fromVersion, Assembly toAssembly, bool ignoredStrongNamed);
}

public enum ReferenceCompatibility
{
    Compatible,
    Mismatch,
    MismatchBreaking
}

public enum VersionCompatibilityMode
{
    Off,
    Loose,
    Strict
}