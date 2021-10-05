using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using RefScout.Analyzer.Analyzers.Environment.Core;
using RefScout.Analyzer.Config.Core;
using RefScout.Analyzer.Helpers;
using RefScout.Analyzer.Resolvers.Strategies;
using RefScout.Analyzer.Resolvers.Strategies.Core;
using RefScout.Analyzer.Resolvers.Strategies.Shared;
using RefScout.Core.Logging;

namespace RefScout.Analyzer.Resolvers;

internal class CoreResolver : Resolver
{
    private readonly CoreConfig _config;
    private readonly CoreRuntime? _runtime;
    private readonly IEnvironment _environment;
    private readonly IFileSystem _fileSystem;
    private readonly TargetFramework _targetFramework;

    public CoreResolver(
        IEnvironment environment,
        IFileSystem fileSystem,
        TargetFramework targetFramework,
        CoreConfig config,
        CoreRuntime? runtime) : base(targetFramework)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _targetFramework = targetFramework ?? throw new ArgumentNullException(nameof(targetFramework));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _runtime = runtime;
    }

    protected override IReadOnlyList<IResolverStrategy> GetResolverStrategies()
    {
        if (_runtime == null || _config.SelfContained)
        {
            if (_config.SelfContained)
            {
                Logger.Info("Application is a single file application, will not search in shared .NET Core folders.");
            }
            else
            {
                Logger.Info("No .NET Core runtime was found, will not search in shared .NET Core folders.");
            }

            return new List<IResolverStrategy>
            {
                new DirectoryResolverStrategy(_fileSystem, SearchDirectories),
                new CoreNuGetPackageResolverStrategy(_environment, _fileSystem, _targetFramework, _config.DepsFile)
            };
        }

        return new List<IResolverStrategy>
        {
            new DirectoryResolverStrategy(_fileSystem, SearchDirectories),
            new CoreNuGetPackageResolverStrategy(_environment, _fileSystem, _targetFramework, _config.DepsFile),
            new DirectoryResolverStrategy(_fileSystem, SearchDirectories)
        };
    }
}