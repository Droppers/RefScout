using System;

namespace RefScout.Analyzer.Analyzers.Compatibility;

public class DefaultVersionComparer : IVersionComparer
{
    private readonly VersionCompatibilityMode _systemVersionMode;

    public DefaultVersionComparer(VersionCompatibilityMode systemVersionMode)
    {
        _systemVersionMode = systemVersionMode;
    }

    public ReferenceCompatibility Compare(Version fromVersion, Assembly toAssembly, bool ignoreStrongNamed)
    {
        _ = fromVersion ?? throw new ArgumentNullException(nameof(fromVersion));
        _ = toAssembly ?? throw new ArgumentNullException(nameof(toAssembly));

        var toVersion = toAssembly.ActualVersion;
        var strongNamed = toAssembly.IsStrongNamed && !ignoreStrongNamed;

        var isUnification = toAssembly.IsUnification && toAssembly.Source == AssemblySource.Gac;
        var originalVersionCompatible = isUnification && toAssembly.OriginalVersion != null &&
                                        AreVersionsCompatible(fromVersion, toVersion, strongNamed);
        if (AreVersionsCompatible(fromVersion, toVersion, strongNamed) || originalVersionCompatible)
        {
            return ReferenceCompatibility.Compatible;
        }

        if (toAssembly.IsSystem || toAssembly.IsNetApi)
        {
            return _systemVersionMode switch
            {
                VersionCompatibilityMode.Loose => IsVersionHigher(fromVersion, toVersion)
                    ? ReferenceCompatibility.Mismatch
                    : ReferenceCompatibility.Compatible,
                VersionCompatibilityMode.Strict => IsVersionHigher(fromVersion, toVersion)
                    ? ReferenceCompatibility.MismatchBreaking
                    : ReferenceCompatibility.Mismatch,
                _ => ReferenceCompatibility.Compatible
            };
        }

        return fromVersion.Major != toVersion.Major || strongNamed
            ? ReferenceCompatibility.MismatchBreaking
            : ReferenceCompatibility.Mismatch;
    }

    private static bool IsVersionHigher(Version version1, Version version2) =>
        version1.Major > version2.Major
        || version1.Major == version2.Major
        && version1.Minor > version2.Minor;

    internal static bool AreVersionsCompatible(Version version1, Version version2, bool strict = false)
    {
        if (strict)
        {
            return version1 == version2;
        }

        return version1.Major == version2.Major && version1.Minor == version2.Minor;
    }
}