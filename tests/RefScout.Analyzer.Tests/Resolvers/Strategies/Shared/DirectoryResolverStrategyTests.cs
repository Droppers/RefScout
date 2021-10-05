using System;
using System.Collections.Generic;
using RefScout.Analyzer.Resolvers.Strategies;
using RefScout.Analyzer.Resolvers.Strategies.Shared;
using Xunit;

namespace RefScout.Analyzer.Tests.Resolvers.Strategies.Shared;

public class DirectoryResolverStrategyTests
{
    private readonly IResolverStrategy _resolver;

    public DirectoryResolverStrategyTests()
    {
        _resolver = new DirectoryResolverStrategy(new FakeFileSystem(), new List<string>
        {
            @"C:\project",
            @"C:\project\probe"
        });
    }

    [Fact]
    public void Test()
    {
        Assert.True(_resolver.Test(AssHelp.Iden(),
            new TargetFramework(NetRuntime.Framework, Version.Parse("4.7.2"))));
    }

    [Fact]
    public void Resolve()
    {
        Assert.Equal(@"C:\project\Application.dll", _resolver.Resolve(AssHelp.Iden("Application"))?.Path);
        Assert.Equal(@"C:\project\en-US\Application.dll",
            _resolver.Resolve(AssHelp.Iden("Application", culture: "en-US"))?.Path);
        Assert.Equal(@"C:\project\Executable.exe", _resolver.Resolve(AssHelp.Iden("Executable"))?.Path);
        Assert.Equal(@"C:\project\probe\Probe.dll", _resolver.Resolve(AssHelp.Iden("Probe"))?.Path);
    }
}