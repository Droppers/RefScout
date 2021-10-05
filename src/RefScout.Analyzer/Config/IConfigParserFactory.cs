using System;
using System.IO.Abstractions;
using RefScout.Analyzer.Config.Core;
using RefScout.Analyzer.Config.Framework;

namespace RefScout.Analyzer.Config;

internal interface IConfigParserFactory
{
    public IConfigParser<TConfig> Create<TConfig>() where TConfig : IConfig;
}

internal class ConfigParserFactory : IConfigParserFactory
{
    private readonly IFileSystem _fileSystem;

    public ConfigParserFactory(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public IConfigParser<TConfig> Create<TConfig>() where TConfig : IConfig
    {
        if (typeof(TConfig) == typeof(CoreConfig))
        {
            return (IConfigParser<TConfig>)new CoreConfigParser(_fileSystem);
        }

        if (typeof(TConfig) == typeof(FrameworkConfig))
        {
            return (IConfigParser<TConfig>)new FrameworkConfigParser(_fileSystem);
        }

        throw new NotSupportedException("Parsing this kind of config is not yet supported.");
    }
}