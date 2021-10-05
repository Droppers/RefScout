using System;
using System.Collections.Generic;

namespace RefScout.Analyzer.Resolvers;

internal interface IResolver : IDisposable
{
    public IReadOnlyList<string> SearchDirectories { get; }

    public AssemblyResolverResult ResolvePath(AssemblyIdentity identity);

    void AddSearchDirectory(string directory);
}

public record AssemblyResolverResult(AssemblySource Source, string? Path = null)
{
    public bool Unification { get; init; }
}