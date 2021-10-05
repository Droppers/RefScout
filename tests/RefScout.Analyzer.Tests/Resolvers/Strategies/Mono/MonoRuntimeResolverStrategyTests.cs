using System;
using System.IO.Abstractions.TestingHelpers;
using RefScout.Analyzer.Resolvers.Strategies;
using RefScout.Analyzer.Resolvers.Strategies.Mono;
using Xunit;

namespace RefScout.Analyzer.Tests.Resolvers.Strategies.Mono;

public class MonoRuntimeResolverStrategyTests
{
    private readonly IResolverStrategy _resolver;

    public MonoRuntimeResolverStrategyTests()
    {
        _resolver = new MonoRuntimeResolverStrategy(new FakeFileSystem(), new FakeMonoRuntimeAnalyzer());
    }

    [Fact]
    public void Test()
    {
        Assert.True(_resolver.Test(
            AssHelp.Iden("System.IO.Compression", token: PublicKeyToken.Parse("969db8053d3322ac")),
            new TargetFramework(NetRuntime.Framework, Version.Parse("2.0"))));

        Assert.False(_resolver.Test(
            AssHelp.Iden("System.IO.Compression", token: PublicKeyToken.Parse("969db8053d3322ac")),
            new TargetFramework(NetRuntime.Standard, Version.Parse("2.0"))));

        Assert.False(_resolver.Test(AssHelp.Iden("System.IO.Compression"),
            new TargetFramework(NetRuntime.Core, Version.Parse("5.0"))));
    }

    [Fact]
    public void Resolve()
    {
        var token = PublicKeyToken.Parse("30ad4fe6b2a6aeed");
        var identity = AssHelp.Iden("System.IO.Compression", "4.0.0.0", "neutral", token);

        Assert.Equal(@"C:\Program Files\Mono\lib\mono\4.5\System.IO.Compression.dll",
            _resolver.Resolve(identity)?.Path);
    }

    [Fact]
    public void Resolve_NotInstalled()
    {
        var resolver = new MonoRuntimeResolverStrategy(new MockFileSystem(), new FakeMonoRuntimeAnalyzer());

        var token = PublicKeyToken.Parse("30ad4fe6b2a6aeed");
        var identity = AssHelp.Iden("System.IO.Compression", "4.0.0.0", "neutral", token);

        Assert.Null(resolver.Resolve(identity));
    }
}