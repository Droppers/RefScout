using System;
using Moq;
using RefScout.Analyzer.Resolvers.Strategies.Framework;
using Xunit;

namespace RefScout.Analyzer.Tests.Resolvers.Strategies.Framework;

public class FusionGacResolverStrategyTests
{
    private readonly FusionGacResolverStrategy _resolver;

    public FusionGacResolverStrategyTests()
    {
        var mock = new Mock<IFusionWrapper>();
        mock.Setup(x =>
                x.QueryAssemblyPath(
                    "Microsoft.DirectX, Version=1.0.2902.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL"))
            .Returns(@"C:\WINDOWS\path\to\Test.dll");
        mock.Setup(x =>
                x.QueryAssemblyPath(
                    "System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=AMD64"))
            .Returns(@"C:\WINDOWS\path\to\amd64.dll");

        _resolver = new FusionGacResolverStrategy(new FakeEnvironment(), new FakeFileSystem(), mock.Object, true);
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
    public void Resolve_ArchitectureDependent()
    {
        var token = PublicKeyToken.Parse("31bf3856ad364e35");
        var identity = AssHelp.Iden("System.Data", "4.0.0.0", "neutral", token);

        Assert.Equal(@"C:\WINDOWS\path\to\amd64.dll", _resolver.Resolve(identity)?.Path);
    }

    [Fact]
    public void Resolve()
    {
        var token = PublicKeyToken.Parse("31bf3856ad364e35");
        var identity = AssHelp.Iden("Microsoft.DirectX", "1.0.2902.0", "neutral", token);

        Assert.Equal(@"C:\WINDOWS\path\to\Test.dll", _resolver.Resolve(identity)?.Path);
    }

    [Fact]
    public void FindFusionPaths()
    {
        Assert.Equal(new[]
        {
            @"C:\WINDOWS\Microsoft.NET\Framework64\v4.0.30319\fusion.dll",
            @"C:\WINDOWS\Microsoft.NET\Framework64\v2.0.50727\fusion.dll"
        }, _resolver.FindFusionPaths());
    }

    [Fact]
    public void Dispose()
    {
        _resolver.Dispose();
    }
}