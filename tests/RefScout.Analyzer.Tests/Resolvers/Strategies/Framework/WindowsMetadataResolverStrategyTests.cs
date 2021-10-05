using System;
using System.IO.Abstractions.TestingHelpers;
using RefScout.Analyzer.Resolvers.Strategies;
using RefScout.Analyzer.Resolvers.Strategies.Framework;
using Xunit;

namespace RefScout.Analyzer.Tests.Resolvers.Strategies.Framework;

public class WindowsMetadataResolverStrategyTests
{
    private readonly IResolverStrategy _resolver;

    public WindowsMetadataResolverStrategyTests()
    {
        _resolver = new WindowsMetadataResolverStrategy(new FakeEnvironment(), new FakeFileSystem());
    }

    [Fact]
    public void Test()
    {
        // TODO: OS check when environment is faked
        Assert.True(_resolver.Test(AssHelp.Iden("System.Windows") with { IsWindowsRuntime = true },
            new TargetFramework(NetRuntime.Silverlight, new Version(5, 1))));

        Assert.False(_resolver.Test(AssHelp.Iden("System.Windows") with { IsWindowsRuntime = false },
            new TargetFramework(NetRuntime.Framework, new Version(5, 1))));
    }

    [Fact]
    public void Resolve_SDKDirectory()
    {
        var identity = AssHelp.Iden("Windows.System.Profile.ProfileRetailInfoContract");
        Assert.Equal(
            @"C:\Program Files (x86)\Windows Kits\10\References\10.0.19041.0\Windows.System.Profile.ProfileRetailInfoContract\1.0.0.0\Windows.System.Profile.ProfileRetailInfoContract.winmd",
            _resolver.Resolve(identity)?.Path);
    }

    [Fact]
    public void Resolve_WithoutSDK()
    {
        var resolver = new WindowsMetadataResolverStrategy(new FakeEnvironment(), new MockFileSystem());
        var identity = AssHelp.Iden("Windows.System.Profile.ProfileRetailInfoContract");
        Assert.Null(resolver.Resolve(identity)?.Path);
    }

    [Fact]
    public void Resolve_System32()
    {
        var identity = AssHelp.Iden("Windows.Globalization");
        Assert.Equal(@"C:\WINDOWS\system32\WinMetadata\Windows.Globalization.winmd",
            _resolver.Resolve(identity)?.Path);
    }

    [Fact]
    public void Resolve_NonExistent()
    {
        var identity = AssHelp.Iden("ThisDoesNotExistAndIfItDoesItsAMiracle");
        Assert.Null(_resolver.Resolve(identity)?.Path);
    }
}