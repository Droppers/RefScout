using System;
using RefScout.Analyzer.Resolvers.Strategies;
using RefScout.Analyzer.Resolvers.Strategies.Framework;
using Xunit;

namespace RefScout.Analyzer.Tests.Resolvers.Strategies.Framework;

public class CorLibResolverStrategyTests
{
    private readonly IResolverStrategy _resolver;

    public CorLibResolverStrategyTests()
    {
        _resolver = new CorLibResolverStrategy(new FakeEnvironment(), new FakeFileSystem());
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
    public void Resolve_Compact()
    {
        var token = PublicKeyToken.Parse("969db8053d3322ac");
        var identity = AssHelp.Iden("mscorlib", "3.5", "neutral", token);

        Assert.Equal(@"C:\Program Files\Microsoft.NET\SDK\CompactFramework\v3.5\WindowsCE\mscorlib.dll",
            _resolver.Resolve(identity)?.Path);
    }

    [Fact]
    public void Resolve_Regular()
    {
        var token = PublicKeyToken.Parse("b77a5c561934e089");

        var identity45 = AssHelp.Iden("mscorlib", "4.5", "neutral", token);
        var identity20 = AssHelp.Iden("mscorlib", "2.0", "neutral", token);
        var identity10 = AssHelp.Iden("mscorlib", "1.1", "neutral", token);

        Assert.Equal(@"C:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\mscorlib.dll",
            _resolver.Resolve(identity45)?.Path);
        Assert.Equal(@"C:\WINDOWS\Microsoft.NET\Framework64\v2.0.50727\mscorlib.dll",
            _resolver.Resolve(identity20)?.Path);
        Assert.Equal(@"C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\mscorlib.dll",
            _resolver.Resolve(identity10)?.Path);
    }

    [Fact]
    public void Resolve_WrongVersion()
    {
        var token = PublicKeyToken.Parse("b77a5c561934e089");
        var identity45 = AssHelp.Iden("mscorlib", "5.5", "neutral", token);
        Assert.Null(_resolver.Resolve(identity45));
    }
}