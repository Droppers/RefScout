using System;
using System.Collections.Generic;
using Moq;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Analyzers.Environment.Core;
using RefScout.Analyzer.Analyzers.Environment.Framework;
using RefScout.Analyzer.Analyzers.Environment.Mono;
using Xunit;

namespace RefScout.Analyzer.Tests.Analyzers.Environment;

public class EnvironmentAnalyzerTests
{
    [Fact]
    public void Analyze()
    {
        var coreMock = new Mock<ICoreRuntimeAnalyzer>();
        coreMock.Setup(x => x.Analyze()).Returns(new CoreRuntimeAnalyzerResult(new List<CoreRuntime>
        {
            new(Version.Parse("3.1.0"), "3.1.0", ""),
            new(Version.Parse("5.1.0"), "5.1.0", "")
        }));

        var frameworkMock = new Mock<IFrameworkRuntimeAnalyzer>();
        frameworkMock.Setup(x => x.Analyze()).Returns(new FrameworkRuntimeAnalyzerResult(new List<FrameworkRuntime>
        {
            new(Version.Parse("3.0"), Version.Parse("3.0")),
            new(Version.Parse("5.0"), Version.Parse("5.0"))
        }));

        var monoMock = new Mock<IMonoRuntimeAnalyzer>();
        monoMock.Setup(x => x.Analyze()).Returns(new MonoRuntimeAnalyzerResult(new List<MonoRuntime>
        {
            new(Version.Parse("6.12.0"), @"C:\Test", new[] { new Version(4, 5), new Version(3, 5) }),
            new(Version.Parse("2.0.0"), @"C:\Test", new[] { new Version(2, 0), new Version(1, 0) })
        }));

        var analyzer = new EnvironmentAnalyzer(new FakeEnvironment(), coreMock.Object, frameworkMock.Object,
            monoMock.Object);
        var result = analyzer.Analyze();
        Assert.NotNull(result.Core);
        Assert.Equal(2, result.Core!.Runtimes.Count);
        Assert.NotNull(result.Framework);
        Assert.Equal(2, result.Framework!.Runtimes.Count);
        Assert.NotNull(result.Mono);
        Assert.Equal(2, result.Mono!.Runtimes.Count);
    }
}