using System;
using RefScout.Analyzer.Config;
using RefScout.Analyzer.Config.Core;
using RefScout.Analyzer.Config.Framework;
using RefScout.Analyzer.Tests.Resolvers.Strategies;
using Xunit;

namespace RefScout.Analyzer.Tests.Config;

public class ConfigParserFactoryTests
{
    [Fact]
    public void Create()
    {
        var factory = new ConfigParserFactory(new FakeFileSystem());
        Assert.IsType<CoreConfigParser>(factory.Create<CoreConfig>());
        Assert.IsType<FrameworkConfigParser>(factory.Create<FrameworkConfig>());
    }

    [Fact]
    public void Create_UnsupportedConfigThrows()
    {
        var factory = new ConfigParserFactory(new FakeFileSystem());
        Assert.Throws<NotSupportedException>(() => { factory.Create<FakeConfig>(); });
    }

    private class FakeConfig : IConfig
    {
        public IConfigErrorReport ErrorReport { get; } = null!;
    }
}