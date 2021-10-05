using System.Collections.Generic;
using RefScout.Analyzer.Resolvers;

namespace RefScout.Analyzer.Tests.Fakes;

internal class FakeResolver : IResolver
{
    private readonly List<string> _searchDirectories = new();

    public IReadOnlyList<string> SearchDirectories => _searchDirectories;

    public AssemblyResolverResult ResolvePath(AssemblyIdentity identity) => null!;

    public void AddSearchDirectory(string directory)
    {
        _searchDirectories.Add(directory);
    }

    public void Dispose() { }
}