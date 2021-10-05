using System;
using System.Collections.Generic;
using System.Linq;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Config;
using RefScout.Analyzer.Readers;
using RefScout.Analyzer.Resolvers;
using RefScout.Core.Logging;

namespace RefScout.Analyzer.Context;

internal abstract class Context : IContext
{
    protected readonly IAssemblyReader Reader;
    protected readonly Dictionary<string, Assembly> Cache;
    private List<Assembly> _assemblies;

    private bool _disposed;

    protected Context(
        IAssemblyReader reader,
        IResolver resolver,
        EnvironmentInfo environmentInfo,
        Assembly entryPoint)
    {
        Resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        EnvironmentInfo = environmentInfo;
        EntryPoint = entryPoint ?? throw new ArgumentNullException(nameof(entryPoint));

        Cache = new Dictionary<string, Assembly>();
        _assemblies = new List<Assembly>();

        Add(entryPoint);
    }

    public abstract IConfig Config { get; }

    public IReadOnlyList<Assembly> Assemblies
    {
        get => _assemblies;
        set => _assemblies = value.ToList();
    }

    public IResolver Resolver { get; }

    public Assembly EntryPoint { get; }
    public EnvironmentInfo EnvironmentInfo { get; }

    public bool Contains(string assemblyName) =>
        _assemblies.Any(a => a.Name == assemblyName);

    public bool Contains(Assembly assembly)
    {
        _ = assembly ?? throw new ArgumentNullException(nameof(assembly));
        return _assemblies.Any(a => a.Name == assembly.Name && a.Version == assembly.Version);
    }

    public void Add(Assembly assembly)
    {
        _ = assembly ?? throw new ArgumentNullException(nameof(assembly));

        // Duplicates are only allowed when an assembly is unreferenced, because in some instances the same
        // unused assembly is found in different probing paths
        if (assembly.IsUnreferenced || !Contains(assembly))
        {
            _assemblies.Add(assembly);
        }
        else
        {
            throw new Exception($"Assembly is already present: {assembly.FullName}");
        }
    }

    public abstract Assembly? Find(AssemblyIdentity identity);
    public abstract Assembly Resolve(AssemblyIdentity identity);

    protected Assembly ResolveFromResolver(AssemblyIdentity identity)
    {
        var result = Resolver.ResolvePath(identity);
        if (result.Source == AssemblySource.NotFound)
        {
            Logger.Error(
                $"Assembly \"{identity.FullName}\" could not be found in the application's working directory, probing directories or global assembly cache");
            return new Assembly(identity, result.Source);
        }

        if (result.Path != null)
        {
            return Reader.ReadOrDefault(result.Path, result.Source, identity) with
            {
                IsUnification = result.Unification,
                OriginalVersion = identity.Version
            };
        }

        // This should technically not happen, but if for whatever reason the resolver returns null it's better to verify it
        Logger.Error($"Path for resolved assembly \"{identity.FullName}\" is null");
        throw new Exception($"Path for resolved assembly \"{identity.FullName}\" is null");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || _disposed)
        {
            return;
        }

        Resolver.Dispose();
        _disposed = true;
    }
}