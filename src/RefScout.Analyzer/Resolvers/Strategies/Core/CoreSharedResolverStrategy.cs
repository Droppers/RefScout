using System.IO;
using System.IO.Abstractions;
using System.Linq;
using RefScout.Analyzer.Analyzers.Environment.Core;

namespace RefScout.Analyzer.Resolvers.Strategies.Core;

public class CoreSharedResolverStrategy : IResolverStrategy
{
    private readonly IFileSystem _fileSystem;
    private readonly CoreRuntime _runtime;
    private readonly string _preferredRuntimePack;

    public CoreSharedResolverStrategy(
        IFileSystem fileSystem,
        CoreRuntime runtime,
        RuntimePack runtimePack)
    {
        _fileSystem = fileSystem;
        _runtime = runtime;
        _preferredRuntimePack = CoreRuntimeAnalyzer.PackToString[runtimePack];
    }

    public bool Test(AssemblyIdentity identity, TargetFramework? targetFramework) =>
        targetFramework?.Runtime is NetRuntime.Core or NetRuntime.Standard;

    public AssemblyResolverResult? Resolve(AssemblyIdentity identity)
    {
        var path = ResolveFromSharedRuntime(identity);
        return path != null
            ? new AssemblyResolverResult(AssemblySource.Shared, path)
            : null;
    }

    private string? ResolveFromSharedRuntime(AssemblyIdentity identity)
    {
        var packs = CoreRuntimeAnalyzer.Packs.ToList();
        packs.Insert(0, _preferredRuntimePack);
        foreach (var pack in packs)
        {
            var packDirectory = Path.Combine(_runtime.Path, pack, _runtime.VersionName);
            if (!_fileSystem.Directory.Exists(packDirectory))
            {
                continue;
            }

            var pathToAssembly = Path.Combine(packDirectory, identity.Name + ".dll");
            if (_fileSystem.File.Exists(pathToAssembly))
            {
                return pathToAssembly;
            }
        }

        return null;
    }
}