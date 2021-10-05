using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using RefScout.Analyzer.Analyzers.Environment.Core;
using RefScout.Analyzer.Resolvers.Strategies;
using RefScout.Analyzer.Resolvers.Strategies.Core;
using Xunit;

namespace RefScout.Analyzer.Tests.Resolvers.Strategies.Core;

public class CoreSharedResolverStrategyTests
{
    [Fact]
    public void Test()
    {
        var resolver = CreateResolver(new Version(5, 0, 0), RuntimePack.Default);
        Assert.True(resolver.Test(AssHelp.Iden("WindowsBase"),
            new TargetFramework(NetRuntime.Core, new Version(5, 1))));
        Assert.True(resolver.Test(AssHelp.Iden("WindowsBase"),
            new TargetFramework(NetRuntime.Core, new Version(5, 1))));
        Assert.False(resolver.Test(AssHelp.Iden("WindowsBase"),
            new TargetFramework(NetRuntime.Framework, new Version(4, 8))));
    }

    [Fact]
    public void Resolve_PreferCoreRuntime()
    {
        var resolver = CreateResolver(new Version(5, 0, 0), RuntimePack.Default);
        var identity = AssHelp.Iden("WindowsBase");
        Assert.Equal(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\5.0.0\WindowsBase.dll",
            resolver.Resolve(identity)?.Path);
    }

    [Fact]
    public void Resolve_PreferDesktopRuntime()
    {
        var resolver = CreateResolver(new Version(5, 0, 0), RuntimePack.WindowsDesktop);
        var identity = AssHelp.Iden("WindowsBase");
        Assert.Equal(@"C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\5.0.0\WindowsBase.dll",
            resolver.Resolve(identity)?.Path);
    }

    [Fact]
    public void Resolve_NotInstalled()
    {
        var resolver = CreateResolver(new Version(5, 0, 0), RuntimePack.WindowsDesktop, new MockFileSystem());
        var identity = AssHelp.Iden("WindowsBase");
        Assert.Null(resolver.Resolve(identity)?.Path);
    }

    //TODO: Test for Linux and MacOSX when environment is faked

    private static IResolverStrategy CreateResolver(
        Version version,
        RuntimePack pack,
        IFileSystem? fileSystem = null)
    {
        var runtime = new CoreRuntime(version, version.ToString(), @"C:\Program Files\dotnet\shared");
        return new CoreSharedResolverStrategy(fileSystem ?? new FakeFileSystem(), runtime, pack);
    }
}