using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using RefScout.Analyzer.Config.Core;
using RefScout.Analyzer.Resolvers.Strategies;
using RefScout.Analyzer.Resolvers.Strategies.Core;
using Xunit;

namespace RefScout.Analyzer.Tests.Resolvers.Strategies.Core;

public class CoreNuGetPackageResolverStrategyTests
{
    [Fact]
    public void Test()
    {
        var resolver = CreateResolver();
        Assert.True(resolver.Test(AssHelp.Iden("WindowsBase"),
            new TargetFramework(NetRuntime.Core, new Version(5, 1))));
        Assert.True(resolver.Test(AssHelp.Iden("WindowsBase"),
            new TargetFramework(NetRuntime.Core, new Version(5, 1))));
        Assert.False(resolver.Test(AssHelp.Iden("WindowsBase"),
            new TargetFramework(NetRuntime.Framework, new Version(4, 8))));
    }

    [Fact]
    public void Resolve()
    {
        var resolver = CreateResolver();
        var identity = AssHelp.Iden("System.IO");
        Assert.Equal(@"c:\users\joery\.nuget\packages\system.io\4.3.0\lib\net462\System.IO.dll",
            resolver.Resolve(identity)?.Path);
    }

    [Fact]
    public void Resolve_Exe()
    {
        var resolver = CreateResolver();
        var identity = AssHelp.Iden("System.Bratwurst");
        Assert.Equal(@"c:\users\joery\.nuget\packages\system.bratwurst\4.3.0\lib\net462\System.Bratwurst.exe",
            resolver.Resolve(identity)?.Path);
    }


    [Fact]
    public void Resolve_NoFiles()
    {
        var resolver = CreateResolver(new MockFileSystem());
        var identity = AssHelp.Iden("WindowsBase");
        Assert.Null(resolver.Resolve(identity)?.Path);
    }

    private static IResolverStrategy CreateResolver(
        IFileSystem? fileSystem = null)
    {
        var depsFile = new DepsFile
        {
            Targets = new Dictionary<string, Dictionary<string, DepsFile.RuntimeInfo>>
            {
                [".NETCoreApp,Version=v5.0"] = new()
                {
                    ["System.IO/4.3.0"] = new DepsFile.RuntimeInfo
                    {
                        Runtime = new Dictionary<string, object>
                        {
                            { "lib/net462/System.IO.dll", null! }
                        }
                    },
                    ["System.Bratwurst/4.3.0"] = new DepsFile.RuntimeInfo
                    {
                        Runtime = new Dictionary<string, object>
                        {
                            { "lib/net462/System.Bratwurst.exe", null! }
                        }
                    }
                }
            },
            Libraries = new Dictionary<string, DepsFile.Library>
            {
                ["System.IO/4.3.0"] = new()
                {
                    Path = "system.io/4.3.0",
                    Type = "package"
                },
                ["System.Bratwurst/4.3.0"] = new()
                {
                    Path = "system.io/4.3.0",
                    Type = "package"
                }
            }
        };

        var target = new TargetFramework(NetRuntime.Core, Version.Parse("5.0"));
        return new CoreNuGetPackageResolverStrategy(new FakeEnvironment(), fileSystem ?? new FakeFileSystem(),
            target, depsFile);
    }
}