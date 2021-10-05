using System;
using System.Collections.Generic;
using RefScout.Analyzer.Resolvers.Strategies;
using RefScout.Core.Logging;

namespace RefScout.Analyzer.Resolvers;

// A universal way of resolving assemblies with support for .NET Core and Framework
internal abstract class Resolver : IResolver
{
    private readonly List<string> _directories;
    private readonly TargetFramework _targetFramework;

    private IReadOnlyList<IResolverStrategy>? _strategies;

    protected Resolver(TargetFramework targetFramework)
    {
        _targetFramework = targetFramework;
        _directories = new List<string>();
    }

    public IReadOnlyList<string> SearchDirectories => _directories;

    public AssemblyResolverResult ResolvePath(AssemblyIdentity identity)
    {
        _ = identity ?? throw new ArgumentNullException(nameof(identity));


        _strategies ??= GetResolverStrategies();

        foreach (var strategy in _strategies)
        {
            if (!strategy.Test(identity, _targetFramework))
            {
                continue;
            }

            var result = TryResolveWithMethod(strategy, identity);
            if (result != null)
            {
                return result;
            }
        }

        return new AssemblyResolverResult(AssemblySource.NotFound);
    }

    public void AddSearchDirectory(string directory)
    {
        if (_directories.Contains(directory))
        {
            return;
        }

        _directories.Add(directory);
    }

    private static AssemblyResolverResult? TryResolveWithMethod(IResolverStrategy method, AssemblyIdentity identity)
    {
        try
        {
            return method.Resolve(identity);
        }
        catch (Exception e)
        {
            Logger.Warn(e, $"Error occurred while running assembly resolver method {method.GetType().Name}");
        }

        return null;
    }

    protected abstract IReadOnlyList<IResolverStrategy> GetResolverStrategies();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposing || _strategies == null)
        {
            return;
        }

        foreach (var method in _strategies)
        {
            method.Dispose();
        }
    }
}