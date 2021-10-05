using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using RefScout.Analyzer.Analyzers.Environment.Mono;
using RefScout.Analyzer.Resolvers.Strategies;
using RefScout.Analyzer.Resolvers.Strategies.Mono;
using RefScout.Analyzer.Resolvers.Strategies.Shared;

namespace RefScout.Analyzer.Resolvers;

internal class MonoResolver : Resolver
{
    private readonly IFileSystem _fileSystem;
    private readonly IMonoRuntimeAnalyzer _monoRuntimeAnalyzer;

    public MonoResolver(
        IFileSystem fileSystem,
        IMonoRuntimeAnalyzer monoRuntimeAnalyzer,
        TargetFramework targetFramework) : base(targetFramework)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _monoRuntimeAnalyzer = monoRuntimeAnalyzer ?? throw new ArgumentNullException(nameof(monoRuntimeAnalyzer));
    }

    protected override IReadOnlyList<IResolverStrategy> GetResolverStrategies() =>
        new List<IResolverStrategy>
        {
            new DirectoryResolverStrategy(_fileSystem, new List<string>(SearchDirectories)),
            new MonoCorLibResolverStrategy(_fileSystem, _monoRuntimeAnalyzer),
            new MonoRuntimeResolverStrategy(_fileSystem, _monoRuntimeAnalyzer),
            new MonoGacResolverStrategy(_fileSystem, _monoRuntimeAnalyzer)
        };
}