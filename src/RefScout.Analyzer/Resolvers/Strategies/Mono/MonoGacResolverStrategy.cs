using System.IO;
using System.IO.Abstractions;
using RefScout.Analyzer.Analyzers.Environment.Mono;

namespace RefScout.Analyzer.Resolvers.Strategies.Mono;

internal class MonoGacResolverStrategy : IResolverStrategy
{
    private readonly IFileSystem _fileSystem;
    private readonly IMonoRuntimeAnalyzer _monoRuntimeAnalyzer;

    public MonoGacResolverStrategy(IFileSystem fileSystem, IMonoRuntimeAnalyzer monoRuntimeAnalyzer)
    {
        _fileSystem = fileSystem;
        _monoRuntimeAnalyzer = monoRuntimeAnalyzer;
    }

    public bool Test(AssemblyIdentity identity, TargetFramework? targetFramework) =>
        targetFramework?.Runtime is NetRuntime.Framework &&
        identity.IsStrongNamed;

    public AssemblyResolverResult? Resolve(AssemblyIdentity identity)
    {
        foreach (var baseDirectory in _monoRuntimeAnalyzer.GetGacPrefixDirectories())
        {
            var gacDirectory = Path.Combine(baseDirectory, "lib", "mono", "gac");
            var fileName = GetAssemblyFile(identity, gacDirectory);

            if (_fileSystem.File.Exists(fileName))
            {
                return new AssemblyResolverResult(AssemblySource.Gac, fileName);
            }
        }

        return null;
    }

    private static string GetAssemblyFile(AssemblyIdentity identity, string gacDirectory) =>
        Path.Combine(gacDirectory, identity.Name, $"{identity.Version}__{identity.PublicKeyToken}",
            identity.Name + ".dll");
}