using System;
using System.Collections.Generic;
using System.Linq;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Analyzers.Environment.Core;
using RefScout.Analyzer.Config.Core;
using RefScout.Analyzer.Readers;
using RefScout.Analyzer.Resolvers;

namespace RefScout.Analyzer.Context;

public record CoreAnalyzerResult(
    IReadOnlyList<Assembly> Assemblies,
    CoreConfig Config,
    CoreRuntime? Runtime,
    EnvironmentInfo EnvironmentInfo) : IAnalyzerResult;

internal class CoreContext : Context, ICoreContext
{
    public CoreContext(
        IResolver resolver,
        IAssemblyReader reader,
        EnvironmentInfo environmentInfo,
        CoreConfig config,
        CoreRuntime? runtime,
        Assembly entryPoint) : base(reader, resolver, environmentInfo, entryPoint)
    {
        Runtime = runtime;
        Config = config;
    }

    public CoreRuntime? Runtime { get; }
    public override CoreConfig Config { get; }

    public override Assembly? Find(AssemblyIdentity identity)
    {
        _ = identity ?? throw new ArgumentNullException(nameof(identity));

        // First try to find an exact version, then fallback to first assembly found
        var assembly =
            Assemblies.SingleOrDefault(a => a.Name == identity.Name && a.ActualVersion == identity.Version);
        return assembly ?? Assemblies.FirstOrDefault(a => a.Name == identity.Name);
    }

    public override Assembly Resolve(AssemblyIdentity identity)
    {
        _ = identity ?? throw new ArgumentNullException(nameof(identity));

        // When only application assemblies are analyzed, system assemblies may still be loaded
        // but are not present in the assemblies list
        if (Cache.TryGetValue(identity.FullName, out var cachedAssembly))
        {
            return cachedAssembly;
        }

        var assembly = ResolveFromResolver(identity);
        Cache[identity.FullName] = assembly;
        return assembly;
    }
}