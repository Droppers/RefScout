using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using RefScout.Analyzer.Analyzers.Environment.Mono;
using RefScout.Analyzer.Tests.Resolvers.Strategies;
using Xunit;

namespace RefScout.Analyzer.Tests.Analyzers.Environment.Mono;

public class MonoRuntimeAnalyzerTests
{
    private readonly IMonoRuntimeAnalyzer _analyzer;

    public MonoRuntimeAnalyzerTests()
    {
        var fileSystem = new FakeFileSystem();
        fileSystem.AddFile(@"C:\Program Files\Mono\lib\mono\4.5\resgen.exe",
            new MockFileData(File.ReadAllBytes(GetPath("resgen.exe"))));
        _analyzer = new MonoRuntimeAnalyzer(new FakeEnvironment(), fileSystem);
    }

    [Fact]
    public void Analyze()
    {
        var result = _analyzer.Analyze();
        Assert.Equal(1, result.Runtimes.Count);
        Assert.Equal(new Version(6, 12, 0, 0), result.Runtimes[0].Version);
        Assert.Equal(new[] { new Version(4, 5) }, result.Runtimes[0].FrameworkVersions);
    }

    [Fact]
    public void GetGacPrefixDirectories()
    {
        Assert.Equal(new[] { "/usr", "/test" }, _analyzer.GetGacPrefixDirectories());
    }

    private static string GetPath(string name) =>
        Path.Combine(AppContext.BaseDirectory, "TestFiles", "Mono", name);
}