using System;
using RefScout.Analyzer.Helpers;
using Xunit;

namespace RefScout.Analyzer.Tests.Helpers;

public class EnvironmentWrapperTests
{
    private readonly IEnvironment _environment;

    public EnvironmentWrapperTests()
    {
        _environment = new EnvironmentWrapper();
    }

    [Fact]
    public void Is64BitOperatingSystem()
    {
        Assert.Equal(Environment.Is64BitOperatingSystem, _environment.Is64BitOperatingSystem);
    }

    [Fact]
    public void OsVersion()
    {
        Assert.Equal(Environment.OSVersion, _environment.OSVersion);
    }

    [Fact]
    public void GetFolderPath()
    {
        Assert.Equal(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory),
            _environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory));
    }

    [Fact]
    public void GetEnvironmentVariable()
    {
        Assert.Equal(Environment.GetEnvironmentVariable("PATH"), _environment.GetEnvironmentVariable("PATH"));
    }
}