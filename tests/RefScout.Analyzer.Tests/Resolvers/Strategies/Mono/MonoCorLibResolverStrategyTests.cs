using System;
using System.IO.Abstractions.TestingHelpers;
using RefScout.Analyzer.Resolvers.Strategies;
using RefScout.Analyzer.Resolvers.Strategies.Mono;
using Xunit;

namespace RefScout.Analyzer.Tests.Resolvers.Strategies.Mono;

public class MonoCorLibResolverStrategyTests
{
    private readonly IResolverStrategy _resolver;

    public MonoCorLibResolverStrategyTests()
    {
        _resolver = new MonoCorLibResolverStrategy(new FakeFileSystem(), new FakeMonoRuntimeAnalyzer());
    }

    [Fact]
    public void Test()
    {
        Assert.True(_resolver.Test(AssHelp.Iden("mscorlib", token: PublicKeyToken.Parse("969db8053d3322ac")),
            new TargetFramework(NetRuntime.Framework, Version.Parse("2.0"))));

        // Not mscorlib
        Assert.False(_resolver.Test(AssHelp.Iden("not-mscorlib", token: PublicKeyToken.Parse("969db8053d3322ac")),
            new TargetFramework(NetRuntime.Framework, Version.Parse("4.7.2"))));

        // No PublicKeyToken
        Assert.False(_resolver.Test(AssHelp.Iden("mscorlib"),
            new TargetFramework(NetRuntime.Framework, Version.Parse("4.7.2"))));

        // Not .NET Framework
        Assert.False(_resolver.Test(AssHelp.Iden("mscorlib"),
            new TargetFramework(NetRuntime.Core, Version.Parse("5.0"))));
    }

    [Fact]
    public void Resolve()
    {
        var token = PublicKeyToken.Parse("b77a5c561934e089");
        var identity = AssHelp.Iden("mscorlib", "4.5", "neutral", token);
        Assert.Equal(@"C:\Program Files\Mono\lib\mono\4.5\mscorlib.dll", _resolver.Resolve(identity)?.Path);
    }

    [Fact]
    public void Resolve_NotInstalled()
    {
        var token = PublicKeyToken.Parse("b77a5c561934e089");
        var identity = AssHelp.Iden("mscorlib", "4.5", "neutral", token);
        var resolver = new MonoCorLibResolverStrategy(new MockFileSystem(), new FakeMonoRuntimeAnalyzer());
        Assert.Null(resolver.Resolve(identity));
    }
}