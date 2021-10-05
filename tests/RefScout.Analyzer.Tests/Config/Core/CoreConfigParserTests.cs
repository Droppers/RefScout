using System;
using System.IO;
using System.IO.Abstractions;
using RefScout.Analyzer.Analyzers.Environment.Core;
using RefScout.Analyzer.Config;
using RefScout.Analyzer.Config.Core;
using Xunit;

namespace RefScout.Analyzer.Tests.Config.Core;

public class CoreConfigParserTests
{
    private readonly IConfigParser<CoreConfig> _parser;

    public CoreConfigParserTests()
    {
        // Use real file system, since it relies on reading and parsing the actual test files
        _parser = new CoreConfigParser(new FileSystem());
    }

    [Fact]
    public void ParseFile()
    {
        var path = GetPath("Test.dll");
        var config = _parser.ParseFile(path, null);

        Assert.False(config.SelfContained);
        Assert.Equal(RuntimePack.WindowsDesktop, config.RuntimePack);
        Assert.Equal(RollForwardBehavior.LatestMajor, config.RollForward);
        Assert.Equal(Version.Parse("5.0.9"), config.TargetRuntimeVersion);

        Assert.NotNull(config.RuntimeConfig);
        Assert.NotNull(config.DepsFile);
    }

    [Fact]
    public void ParseFile_DoesNotCryWhenItCantFindConfig()
    {
        var path = GetPath("PleaseDoNotCry.dll");
        _parser.ParseFile(path, null);
    }

    [Fact]
    public void ParseFile_DoesNotCryWhenConfigIsInvalid()
    {
        var path = GetPath("Invalid.dll");
        _parser.ParseFile(path, null);
    }

    private static string GetPath(string name) =>
        Path.Combine(AppContext.BaseDirectory, "TestFiles", "CoreConfigs", name);
}