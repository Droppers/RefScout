using System;
using System.IO.Abstractions.TestingHelpers;
using RefScout.Analyzer.Resolvers.Strategies;
using RefScout.Analyzer.Resolvers.Strategies.Framework;
using Xunit;

namespace RefScout.Analyzer.Tests.Resolvers.Strategies.Framework;

public class SilverlightResolverStrategyTests
{
    private readonly IResolverStrategy _resolver;

    public SilverlightResolverStrategyTests()
    {
        _resolver = new SilverlightResolverStrategy(new FakeEnvironment(), new FakeFileSystem(),
            new TargetFramework(NetRuntime.Silverlight, new Version(5, 1)));
    }

    [Fact]
    public void Test()
    {
        // TODO: OS check when environment is faked
        Assert.True(_resolver.Test(AssHelp.Iden("System.Windows"),
            new TargetFramework(NetRuntime.Silverlight, new Version(5, 1))));

        // Not Silverlight
        Assert.False(_resolver.Test(AssHelp.Iden("System.Windows"),
            new TargetFramework(NetRuntime.Framework, new Version(5, 1))));
    }

    [Fact]
    public void Resolve_SameVersion()
    {
        var identity = AssHelp.Iden("System.Windows");
        Assert.Equal(@"C:\Program Files (x86)\Microsoft Silverlight\5.1.50918.0\System.Windows.dll",
            _resolver.Resolve(identity)?.Path);
    }

    [Fact]
    public void Resolve_AlternativeVersion()
    {
        var identity = AssHelp.Iden("System.ServiceModel");
        Assert.Equal(@"C:\Program Files\Microsoft Silverlight\4.1.10329.0\System.ServiceModel.dll",
            _resolver.Resolve(identity)?.Path);
    }

    [Fact]
    public void Resolve_SilverLightNotInstalled()
    {
        var resolver = new SilverlightResolverStrategy(new FakeEnvironment(), new MockFileSystem(),
            new TargetFramework(NetRuntime.Silverlight, new Version(5, 1)));
        var identity = AssHelp.Iden("System.ServiceModel");
        Assert.Null(resolver.Resolve(identity)?.Path);
    }
}