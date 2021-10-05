using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace RefScout.Analyzer.Resolvers.Strategies.Shared;

public class DirectoryResolverStrategy : IResolverStrategy
{
    private readonly IFileSystem _fileSystem;
    private readonly IReadOnlyList<string> _directories;

    public DirectoryResolverStrategy(IFileSystem fileSystem, IEnumerable<string> directories)
    {
        _fileSystem = fileSystem;
        _directories = directories.ToList();
    }

    public bool Test(AssemblyIdentity identity, TargetFramework? targetFramework) => true;

    public AssemblyResolverResult? Resolve(AssemblyIdentity identity)
    {
        var path = _directories
            .Select(directory => ResolverHelper.SearchInDirectory(_fileSystem, identity, directory))
            .FirstOrDefault(file => file != null);

        return path != null ? new AssemblyResolverResult(AssemblySource.Local, path) : null;
    }
}