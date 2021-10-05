using System;
using Moq;
using RefScout.Analyzer.Resolvers.Strategies;
using RefScout.Analyzer.Resolvers.Strategies.Framework;
using RefScout.IPC.Client;
using Xunit;

namespace RefScout.Analyzer.Tests.Resolvers.Strategies.Framework;

public class FrameworkProxyGacResolverStrategyTests : IDisposable
{
    private readonly Mock<IIpcClient> _mock;
    private readonly IResolverStrategy _resolver;

    public FrameworkProxyGacResolverStrategyTests()
    {
        _mock = new Mock<IIpcClient>();
        _mock.Setup(x => x.Send("will-throw, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"))
            .Throws<TimeoutException>();
        _mock.Setup(x => x.Send("will-fail, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"))
            .Returns("error");
        _mock.Setup(x =>
                x.Send("Microsoft.DirectX, Version=1.0.2902.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"))
            .Returns(@"1.2.3.4|C:\WINDOWS\path\to\Test.dll");
        _mock.Setup(m => m.Start(It.IsAny<string>(), null));
        _resolver = new FrameworkProxyGacResolverStrategy(new FakeEnvironment(), new FakeFileSystem(), _mock.Object);
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

        Assert.Equal(@"C:\WINDOWS\path\to\Test.dll", _resolver.Resolve(identity)?.Path);

        _mock.Verify(m => m.Start(It.IsAny<string>(), null), Times.Once);
    }

    [Fact]
    public void Resolve_WillFail()
    {
        var token = PublicKeyToken.Parse("31bf3856ad364e35");
        var identity = AssHelp.Iden("will-fail", "1.0.0.0", "neutral", token);

        Assert.Null(_resolver.Resolve(identity)?.Path);
    }

    [Fact]
    public void Resolve_WillThrow()
    {
        var token = PublicKeyToken.Parse("31bf3856ad364e35");
        var identity = AssHelp.Iden("will-throw", "1.0.0.0", "neutral", token);

        Assert.Null(_resolver.Resolve(identity)?.Path);
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        _resolver.Dispose();
        _mock.Verify(m => m.Dispose(), Times.Never);
    }

    public void Dispose()
    {
        _resolver.Dispose();
    }
}