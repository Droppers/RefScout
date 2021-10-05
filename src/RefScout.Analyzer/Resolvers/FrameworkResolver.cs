using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using RefScout.Analyzer.Helpers;
using RefScout.Analyzer.Resolvers.Strategies;
using RefScout.Analyzer.Resolvers.Strategies.Framework;
using RefScout.Analyzer.Resolvers.Strategies.Shared;

namespace RefScout.Analyzer.Resolvers;

internal class FrameworkResolver : Resolver
{
    private readonly IEnvironment _environment;
    private readonly IFileSystem _fileSystem;
    private readonly TargetFramework _targetFramework;
    private readonly bool _is64Bit;

    public FrameworkResolver(
        IEnvironment environment,
        IFileSystem fileSystem,
        TargetFramework targetFramework,
        bool is64Bit) : base(targetFramework)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _targetFramework = targetFramework ?? throw new ArgumentNullException(nameof(targetFramework));
        _is64Bit = is64Bit;
    }

    protected override IReadOnlyList<IResolverStrategy> GetResolverStrategies() =>
        new List<IResolverStrategy>
        {
            new WindowsMetadataResolverStrategy(_environment, _fileSystem),
            new SilverlightResolverStrategy(_environment, _fileSystem, _targetFramework),
            new CorLibResolverStrategy(_environment, _fileSystem),
            new FusionGacResolverStrategy(_environment, _fileSystem, _is64Bit),
            new FileSystemGacResolverStrategy(_environment, _fileSystem),
            new DirectoryResolverStrategy(_fileSystem, new List<string>(SearchDirectories)),
            new FrameworkProxyGacResolverStrategy(_environment, _fileSystem)
        };
}