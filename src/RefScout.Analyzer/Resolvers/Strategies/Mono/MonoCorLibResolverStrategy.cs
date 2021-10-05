using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using RefScout.Analyzer.Analyzers.Environment.Mono;

namespace RefScout.Analyzer.Resolvers.Strategies.Mono;

internal class MonoCorLibResolverStrategy : IResolverStrategy
{
    private const string CorLibName = "mscorlib";
    private readonly IFileSystem _fileSystem;
    private readonly IMonoRuntimeAnalyzer _monoRuntimeAnalyzer;

    public MonoCorLibResolverStrategy(IFileSystem fileSystem, IMonoRuntimeAnalyzer monoRuntimeAnalyzer)
    {
        _fileSystem = fileSystem;
        _monoRuntimeAnalyzer = monoRuntimeAnalyzer;
    }

    public bool Test(AssemblyIdentity identity, TargetFramework? targetFramework) =>
        identity.Name == CorLibName && targetFramework?.Runtime is NetRuntime.Framework &&
        identity.IsStrongNamed;

    public AssemblyResolverResult? Resolve(AssemblyIdentity identity)
    {
        var possibleVersions = GetPossibleMonoVersions(identity.Version).ToList();
        foreach (var baseDirectory in _monoRuntimeAnalyzer.GetRuntimePrefixDirectories())
        {
            foreach (var version in possibleVersions)
            {
                var fileName = Path.Combine(baseDirectory, "lib", "mono", version, "mscorlib.dll");
                if (_fileSystem.File.Exists(fileName))
                {
                    return new AssemblyResolverResult(AssemblySource.Gac, fileName);
                }
            }
        }

        return null;
    }

    private static IEnumerable<string> GetPossibleMonoVersions(Version version)
    {
        switch (version.Major)
        {
            case 1:
                yield return "1.0";
                break;
            case 2 when version.MajorRevision == 5:
                yield return "2.1";
                break;
            case 2:
                yield return "2.0";
                break;
            case 4:
                yield return "4.5";
                yield return "4.0";
                break;
        }
    }
}