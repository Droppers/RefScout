using System;
using System.IO.Abstractions.TestingHelpers;
using RefScout.Analyzer.Resolvers.Strategies;
using RefScout.Analyzer.Resolvers.Strategies.Mono;
using Xunit;

namespace RefScout.Analyzer.Tests.Resolvers.Strategies.Mono;

public class MonoGacResolverStrategyTests
{
    private readonly IResolverStrategy _resolver;

    public MonoGacResolverStrategyTests()
    {
        _resolver = new MonoGacResolverStrategy(new FakeFileSystem(), new FakeMonoRuntimeAnalyzer());
    }

    [Fact]
    public void Test()
    {
        Assert.True(_resolver.Test(AssHelp.Iden("mscorlib", token: PublicKeyToken.Parse("969db8053d3322ac")),
            new TargetFramework(NetRuntime.Framework, Version.Parse("2.0"))));

        Assert.False(_resolver.Test(AssHelp.Iden("mscorlib", token: PublicKeyToken.Parse("969db8053d3322ac")),
            new TargetFramework(NetRuntime.Standard, Version.Parse("2.0"))));

        Assert.False(_resolver.Test(AssHelp.Iden("mscorlib"),
            new TargetFramework(NetRuntime.Core, Version.Parse("5.0"))));
    }

    [Fact]
    public void Resolve()
    {
        var token = PublicKeyToken.Parse("30ad4fe6b2a6aeed");
        var identity = AssHelp.Iden("Newtonsoft.Json", "12.0.0.0", "neutral", token);

        Assert.Equal(
            @"C:\Program Files\Mono\lib\mono\gac\Newtonsoft.Json\12.0.0.0__30ad4fe6b2a6aeed\Newtonsoft.Json.dll",
            _resolver.Resolve(identity)?.Path);
    }

    [Fact]
    public void Resolve_NotInstalled()
    {
        var token = PublicKeyToken.Parse("30ad4fe6b2a6aeed");
        var identity = AssHelp.Iden("Newtonsoft.Json", "12.0.0.0", "neutral", token);
        var resolver = new MonoGacResolverStrategy(new MockFileSystem(), new FakeMonoRuntimeAnalyzer());
        Assert.Null(resolver.Resolve(identity));
    }
}