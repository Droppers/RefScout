using System;
using RefScout.Analyzer.Resolvers.Strategies;
using RefScout.Analyzer.Resolvers.Strategies.Framework;
using Xunit;

namespace RefScout.Analyzer.Tests.Resolvers.Strategies.Framework;

public class FileSystemGacResolverStrategyTests
{
    private readonly IResolverStrategy _resolver;

    public FileSystemGacResolverStrategyTests()
    {
        _resolver = new FileSystemGacResolverStrategy(new FakeEnvironment(), new FakeFileSystem());
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
        var token = PublicKeyToken.Parse("31bf3856ad364e35");
        var identity = AssHelp.Iden("Microsoft.DirectX", "1.0.2902.0", "neutral", token);

        Assert.Equal(@"C:\WINDOWS\assembly\GAC\Microsoft.DirectX\1.0.2902.0__31bf3856ad364e35\Microsoft.DirectX.dll",
            _resolver.Resolve(identity)?.Path);
    }

    [Fact]
    public void Resolve_MicrosoftNet()
    {
        var token = PublicKeyToken.Parse("b03f5f7f11d50a3a");
        var identity = AssHelp.Iden("System.ServiceModel.Http", "4.0.0.0", "neutral", token);

        Assert.Equal(
            @"C:\WINDOWS\Microsoft.NET\assembly\GAC_MSIL\System.ServiceModel.Http\v4.0_4.0.0.0__b03f5f7f11d50a3a\System.ServiceModel.Http.dll",
            _resolver.Resolve(identity)?.Path);
    }
}