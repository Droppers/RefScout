using System.IO;
using System.IO.Abstractions;
using RefScout.Analyzer.Analyzers.Environment.Mono;

namespace RefScout.Analyzer.Resolvers.Strategies.Mono;

internal class MonoRuntimeResolverStrategy : IResolverStrategy
{
    private static readonly string[] SubDirectories = { "", "Facades" };

    private readonly IFileSystem _fileSystem;
    private readonly IMonoRuntimeAnalyzer _monoRuntimeAnalyzer;

    public MonoRuntimeResolverStrategy(IFileSystem fileSystem, IMonoRuntimeAnalyzer monoRuntimeAnalyzer)
    {
        _fileSystem = fileSystem;
        _monoRuntimeAnalyzer = monoRuntimeAnalyzer;
    }

    public bool Test(AssemblyIdentity identity, TargetFramework? targetFramework) =>
        targetFramework?.Runtime is NetRuntime.Framework &&
        identity.IsStrongNamed;

    public AssemblyResolverResult? Resolve(AssemblyIdentity identity)
    {
        foreach (var baseDirectory in _monoRuntimeAnalyzer.GetRuntimePrefixDirectories())
        {
            foreach (var version in MonoRuntimeAnalyzer.PossibleVersionNames)
            {
                foreach (var subDirectory in SubDirectories)
                {
                    var fileName = Path.Combine(baseDirectory, "lib", "mono", version, subDirectory,
                        identity.Name + ".dll");
                    if (_fileSystem.File.Exists(fileName))
                    {
                        return new AssemblyResolverResult(AssemblySource.Gac, fileName);
                    }
                }
            }
        }

        return null;
    }
}