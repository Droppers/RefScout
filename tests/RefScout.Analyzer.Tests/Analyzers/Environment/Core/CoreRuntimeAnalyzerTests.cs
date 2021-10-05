using System;
using System.Linq;
using RefScout.Analyzer.Analyzers.Environment.Core;
using RefScout.Analyzer.Config.Core;
using RefScout.Analyzer.Tests.Resolvers.Strategies;
using Xunit;

namespace RefScout.Analyzer.Tests.Analyzers.Environment.Core;

public class CoreRuntimeAnalyzerTests
{
    private readonly ICoreRuntimeAnalyzer _analyzer;

    public CoreRuntimeAnalyzerTests()
    {
        _analyzer = new CoreRuntimeAnalyzer(new FakeEnvironment(), new FakeFileSystem());
    }

    [Theory]
    [InlineData(RollForwardBehavior.LatestPatch, "2.1.0", "2.1.9",
        new[] { "1.1.17", "2.1.9", "2.2.0", "2.2.1", "2.2.5", "2.3.1", "3.0.0" })]
    [InlineData(RollForwardBehavior.Minor, "2.1.0", "2.2.5",
        new[] { "1.1.17", "2.2.0", "2.2.1", "2.2.5", "2.3.1", "3.0.0" })]
    [InlineData(RollForwardBehavior.Minor, "2.2.1", "2.2.5",
        new[] { "1.1.17", "2.2.0", "2.2.1", "2.2.5", "2.3.1", "3.0.0" })]
    [InlineData(RollForwardBehavior.Major, "2.1.9", "2.2.5",
        new[] { "1.1.17", "2.2.0", "2.2.1", "2.2.5", "2.3.1", "3.0.5", "3.0.0", "3.2.5", "5.0" })]
    [InlineData(RollForwardBehavior.Major, "2.2.5", "3.0.5",
        new[] { "1.1.17", "3.0.5", "3.0.0", "3.2.5", "5.0.0" })]
    [InlineData(RollForwardBehavior.LatestMinor, "3.0.5", "3.2.5",
        new[] { "1.1.17", "3.0.5", "3.1.0", "3.2.0", "3.2.5", "5.0.0", "6.0.0" })]
    [InlineData(RollForwardBehavior.LatestMajor, "3.2.5", "6.0.0",
        new[] { "1.1.17", "3.0.5", "3.0.0", "3.2.5", "5.0.0", "6.0.0" })]
    [InlineData(RollForwardBehavior.Disable, "3.0.5", "3.0.5",
        new[] { "1.1.17", "3.0.5", "3.1.0", "3.2.0", "3.2.5", "5.0.0", "6.0.0" })]
    public void FindRuntime_FromList(
        RollForwardBehavior rollForward,
        string requestedVersion,
        string expectedVersion,
        string[] availableVersions)
    {
        var version = Version.Parse(requestedVersion);
        var actual = _analyzer.FindRuntime(version,
            availableVersions.Select(x => new CoreRuntime(Version.Parse(x), x, "")), rollForward);

        Assert.NotNull(actual);
        Assert.Equal(expectedVersion, actual!.VersionName);
    }

    [Fact]
    public void FindRuntime_FromInstalled()
    {
        var actual = _analyzer.FindRuntime(Version.Parse("3.1.0"), RollForwardBehavior.LatestMajor);
        Assert.Equal("6.0.0-preview.7.21377.19", actual!.VersionName);
    }

    [Fact]
    public void LocateRuntimeVersions()
    {
        var actual = _analyzer.LocateAll();

        Assert.Equal(3, actual.Count);

        Assert.Equal("3.1.0", actual[0].VersionName);
        Assert.Equal(2, actual[0].Packs.Count);

        Assert.Equal("5.0.0", actual[1].VersionName);
        Assert.Equal(2, actual[1].Packs.Count);

        Assert.Equal("6.0.0-preview.7.21377.19", actual[2].VersionName);
        Assert.Equal(7, actual[2].Preview);
        Assert.Equal(21377, actual[2].PreviewBuild);
        Assert.Single(actual[2].Packs);
    }
}