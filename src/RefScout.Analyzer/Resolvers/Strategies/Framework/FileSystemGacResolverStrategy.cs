using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using RefScout.Analyzer.Helpers;

namespace RefScout.Analyzer.Resolvers.Strategies.Framework;

internal class FileSystemGacResolverStrategy : IResolverStrategy
{
    private readonly IEnvironment _environment;
    private readonly IFileSystem _fileSystem;

    public FileSystemGacResolverStrategy(IEnvironment environment, IFileSystem fileSystem)
    {
        _environment = environment;
        _fileSystem = fileSystem;
    }

    public bool Test(AssemblyIdentity identity, TargetFramework? targetFramework) =>
        targetFramework?.Runtime is NetRuntime.Framework && _environment.OSVersion.Platform is PlatformID.Win32NT &&
        identity.IsStrongNamed;

    public AssemblyResolverResult? Resolve(AssemblyIdentity identity)
    {
        var path = GetAssemblyInGac(identity);
        return path != null ? new AssemblyResolverResult(AssemblySource.Gac, path) : null;
    }

    private IReadOnlyList<string> GetGacPaths()
    {
        var paths = new List<string>();
        var windir = _environment.GetFolderPath(Environment.SpecialFolder.Windows);
        paths.Add(Path.Combine(windir, "assembly"));
        paths.Add(Path.Combine(windir, "Microsoft.NET", "assembly"));
        return paths;
    }

    private string? GetAssemblyInGac(AssemblyIdentity identity)
    {
        var gacPaths = GetGacPaths();
        var gacs = new[] { "GAC_MSIL", "GAC_32", "GAC_64", "GAC" };
        var prefixes = new[] { string.Empty, "v4.0_" };

        for (var i = 0; i < gacPaths.Count; i++)
        {
            foreach (var gac in gacs)
            {
                var pathToGac = Path.Combine(gacPaths.ElementAt(i), gac);
                var file = GetAssemblyFile(identity, prefixes[i], pathToGac);
                if (_fileSystem.Directory.Exists(pathToGac) && _fileSystem.File.Exists(file))
                {
                    return file;
                }
            }
        }

        return null;
    }

    private static string GetAssemblyFile(AssemblyIdentity identity, string? prefix, string gac)
    {
        var gacFolder = new StringBuilder()
            .Append(prefix)
            .Append(identity.Version);
        gacFolder.Append("__");
        gacFolder.Append(identity.PublicKeyToken);

        return Path.Combine(gac, identity.Name, gacFolder.ToString(), identity.Name + ".dll");
    }
}