using System;
using RefScout.Analyzer.Analyzers.Environment.Core;
using Xunit;

namespace RefScout.Analyzer.Tests.Analyzers.Environment.Core;

public class CoreRuntimeTests
{
    [Fact]
    public void IsPreview()
    {
        var runtime = new CoreRuntime(Version.Parse("5.0.0"), 1, -1, "5.0.0-preview-1", "", true);
        Assert.True(runtime.IsPreview);
    }

    [Fact]
    public void ToString_x64()
    {
        var runtime = new CoreRuntime(Version.Parse("5.0.0"), 1, -1, "5.0.0-preview-1", "", true);
        Assert.Equal("5.0.0-preview-1", runtime.ToString());
    }

    [Fact]
    public void ToString_x86()
    {
        var runtime = new CoreRuntime(Version.Parse("5.0.0"), 1, -1, "5.0.0-preview-1", "", false);
        Assert.Equal("5.0.0-preview-1 (x86)", runtime.ToString());
    }

    [Fact]
    public void Equals_Test()
    {
        var runtimeA = new CoreRuntime(Version.Parse("5.0.0"), 1, -1, "5.0.0-preview-1", "", false);
        var runtimeB = new CoreRuntime(Version.Parse("5.0.0"), 1, -1, "5.0.0-preview-1", "", false);
        Assert.True(runtimeA.Equals(runtimeB));
    }
}