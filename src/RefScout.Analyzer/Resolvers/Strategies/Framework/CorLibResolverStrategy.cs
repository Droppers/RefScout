using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using RefScout.Analyzer.Helpers;

namespace RefScout.Analyzer.Resolvers.Strategies.Framework;

internal class CorLibResolverStrategy : IResolverStrategy
{
    private const string CorLibName = "mscorlib";
    private static readonly PublicKeyToken CompactPublicKeyToken = PublicKeyToken.Parse("969db8053d3322ac");
    private readonly IEnvironment _environment;
    private readonly IFileSystem _fileSystem;

    public CorLibResolverStrategy(IEnvironment environment, IFileSystem fileSystem)
    {
        _environment = environment;
        _fileSystem = fileSystem;
    }

    public bool Test(AssemblyIdentity identity, TargetFramework? targetFramework) =>
        identity.Name == CorLibName && targetFramework?.Runtime is NetRuntime.Framework &&
        _environment.OSVersion.Platform == PlatformID.Win32NT &&
        identity.IsStrongNamed;

    public AssemblyResolverResult? Resolve(AssemblyIdentity identity)
    {
        var path = GetCorLib(identity);
        return path != null ? new AssemblyResolverResult(AssemblySource.Gac, path) : null;
    }

    private string? GetCorLib(AssemblyIdentity identity)
    {
        var path = GetMscorlibBasePath(identity.Version, identity.PublicKeyToken);
        if (path == null)
        {
            return null;
        }

        var file = Path.Combine(path, "mscorlib.dll");
        return _fileSystem.File.Exists(file) ? file : null;
    }

    private string? GetMscorlibBasePath(Version version, PublicKeyToken publicKeyToken)
    {
        if (publicKeyToken == CompactPublicKeyToken)
        {
            var programFiles = _environment.Is64BitOperatingSystem
                ? _environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
                : _environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var cfPath = $@"Microsoft.NET\SDK\CompactFramework\v{version.Major}.{version.Minor}\WindowsCE\";
            var cfBasePath = Path.Combine(programFiles, cfPath);
            if (_fileSystem.Directory.Exists(cfBasePath))
            {
                return cfBasePath;
            }
        }
        else
        {
            var rootPath = Path.Combine(_environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "Microsoft.NET");
            string[] frameworkPaths =
            {
                Path.Combine(rootPath, "Framework64"),
                Path.Combine(rootPath, "Framework")
            };

            var folder = version.Major switch
            {
                1 => version.MajorRevision == 3300 ? "v1.0.3705" : "v1.1.4322",
                2 => "v2.0.50727",
                4 => "v4.0.30319",
                _ => null
            };
            if (folder != null)
            {
                return frameworkPaths
                    .Select(path => Path.Combine(path, folder))
                    .FirstOrDefault(_fileSystem.Directory.Exists);
            }
        }

        return null;
    }
}