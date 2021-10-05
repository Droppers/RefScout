using System;
using Xunit;

namespace RefScout.Analyzer.Tests;

public class TargetFrameworkTests
{
    [Theory]
    [InlineData(".NETFramework,Version=v4.7.2", NetRuntime.Framework, "4.7.2")]
    [InlineData(".NETCoreApp,Version=v5.0", NetRuntime.Core, "5.0")]
    [InlineData(".NETCore,Version=v3.1", NetRuntime.Core, "3.1")]
    [InlineData(".NETPortable,Version=v4.0", NetRuntime.Framework, "4.0")]
    [InlineData(".NETStandard,Version=2.1", NetRuntime.Standard, "2.1")]
    [InlineData("Silverlight,Version=v5.0", NetRuntime.Silverlight, "5.0")]
    [InlineData(".NETPortable,Version=v4.5,Profile=Profile259", NetRuntime.Framework, "4.5")]
    public void Parse(string identifier, NetRuntime expectedRuntime, string expectedVersion)
    {
        var framework = TargetFramework.Parse(identifier);
        Assert.Equal(expectedRuntime, framework.Runtime);
        Assert.Equal(new Version(expectedVersion), framework.Version);
    }


    [Fact]
    public void Parse_InvalidThrowsException()
    {
        Assert.Throws<Exception>(() => { TargetFramework.Parse("ThisIsNoValid,Version=v4.7.2"); });
    }

    [Theory]
    [InlineData(".NETFramework,Version=v4.7.2", ".NETFramework,Version=v4.7.2")]
    [InlineData(".NETCoreApp,Version=v5.0", ".NETCoreApp,Version=v5.0")]
    [InlineData(".NETCoreApp,Version=v5.0.0", ".NETCoreApp,Version=v5.0")]
    [InlineData(".NETCore,Version=v3.1", ".NETCoreApp,Version=v3.1")]
    public void Id(string identifier, string expectedId)
    {
        var framework = TargetFramework.Parse(identifier);
        Assert.Equal(expectedId, framework.Id);
    }

    [Theory]
    [InlineData(".NETFramework,Version=v4.7.2", "Framework 4.7.2")]
    [InlineData(".NETCoreApp,Version=v5.0", "5.0")]
    [InlineData(".NETCore,Version=v3.1", "Core 3.1")]
    public void ShortName_And_ToString(string identifier, string expectedId)
    {
        var framework = TargetFramework.Parse(identifier);
        Assert.Equal(expectedId, framework.ShortName);
        Assert.Equal($".NET {expectedId}", framework.ToString());
    }
}