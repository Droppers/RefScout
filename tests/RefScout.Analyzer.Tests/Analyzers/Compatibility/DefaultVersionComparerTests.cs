using System;
using RefScout.Analyzer.Analyzers.Compatibility;
using Xunit;

namespace RefScout.Analyzer.Tests.Analyzers.Compatibility;

public class DefaultVersionComparerTests
{
    private static readonly PublicKeyToken PublicKeyToken = PublicKeyToken.Parse("969db8053d3322ac");

    [Fact]
    public void Compare_Compatible()
    {
        var comparer = new DefaultVersionComparer(VersionCompatibilityMode.Strict);
        var assembly = AssHelp.Ass("Test", "5.0.0.0");
        var result = comparer.Compare(new Version("5.0.0.0"), assembly, false);
        Assert.Equal(ReferenceCompatibility.Compatible, result);
    }

    [Fact]
    public void Compare_ConflictBreaking()
    {
        var comparer = new DefaultVersionComparer(VersionCompatibilityMode.Strict);
        var assembly = AssHelp.Ass("Test", "5.0.0.0");
        var result = comparer.Compare(new Version("4.0.0.0"), assembly, false);
        Assert.Equal(ReferenceCompatibility.MismatchBreaking, result);
    }

    [Fact]
    public void Compare_ConflictInformative()
    {
        var comparer = new DefaultVersionComparer(VersionCompatibilityMode.Strict);
        var assembly = AssHelp.Ass("Test", "4.5.0.0");
        var result = comparer.Compare(new Version("4.0.0.0"), assembly, false);
        Assert.Equal(ReferenceCompatibility.Mismatch, result);
    }

    [Fact]
    public void Compare_NetApiConflictBreaking()
    {
        var comparer = new DefaultVersionComparer(VersionCompatibilityMode.Strict);
        var assemblyBreaking = AssHelp.Ass("Test", "4.0.0.0") with
        {
            IsNetApi = true
        };
        var breaking = comparer.Compare(new Version("5.0.0.0"), assemblyBreaking, false);

        var assemblyNotBreaking = AssHelp.Ass("Test", "5.0.0.0") with
        {
            IsSystem = true
        };
        var notBreaking = comparer.Compare(new Version("2.0.0.0"), assemblyNotBreaking, false);

        Assert.Equal(ReferenceCompatibility.MismatchBreaking, breaking);
        Assert.Equal(ReferenceCompatibility.Mismatch, notBreaking);
    }

    [Fact]
    public void Compare_UnificationNotError()
    {
        var comparer = new DefaultVersionComparer(VersionCompatibilityMode.Strict);
        var assembly = AssHelp.Ass("Test", "4.0.0.0", source: AssemblySource.Gac) with
        {
            IsSystem = true,
            IsUnification = true,
            OriginalVersion = new Version("4.2.0.0")
        };
        var result = comparer.Compare(new Version("4.2.0.0"), assembly, false);

        Assert.Equal(ReferenceCompatibility.Compatible, result);
    }

    [Fact]
    public void Compare_StrongNamedExactMatch()
    {
        var comparer = new DefaultVersionComparer(VersionCompatibilityMode.Strict);
        var assembly = AssHelp.Ass("Test", "4.0.0.1", PublicKeyToken, AssemblySource.Gac);
        var result = comparer.Compare(new Version("4.0.0.0"), assembly, false);

        Assert.Equal(ReferenceCompatibility.MismatchBreaking, result);
    }


    [InlineData(VersionCompatibilityMode.Off, "2.0.0.0", "4.0.0.0", ReferenceCompatibility.Compatible)]
    [InlineData(VersionCompatibilityMode.Loose, "2.0.0.0", "4.0.0.0", ReferenceCompatibility.Compatible)]
    [InlineData(VersionCompatibilityMode.Strict, "2.0.0.0", "4.0.0.0", ReferenceCompatibility.Mismatch)]
    [InlineData(VersionCompatibilityMode.Off, "5.0.0.0", "4.0.0.0", ReferenceCompatibility.Compatible)]
    [InlineData(VersionCompatibilityMode.Loose, "5.0.0.0", "4.0.0.0", ReferenceCompatibility.Mismatch)]
    [InlineData(VersionCompatibilityMode.Strict, "5.0.0.0", "4.0.0.0", ReferenceCompatibility.MismatchBreaking)]
    [Theory]
    public void Compare_SystemMismatchMode(
        VersionCompatibilityMode mode,
        string referencedVersion,
        string assemblyVersion,
        ReferenceCompatibility expected)
    {
        var comparer = new DefaultVersionComparer(mode);
        var assembly = AssHelp.Ass("Test", assemblyVersion, PublicKeyToken, AssemblySource.Gac) with
        {
            IsSystem = true
        };
        var result = comparer.Compare(new Version(referencedVersion), assembly, false);
        Assert.Equal(expected, result);
    }
}