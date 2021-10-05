using System;
using System.IO;
using System.IO.Abstractions;
using RefScout.Analyzer.Helpers;

namespace RefScout.Analyzer.Resolvers.Strategies.Framework;

internal class SilverlightResolverStrategy : IResolverStrategy
{
    private readonly IEnvironment _environment;
    private readonly IFileSystem _fileSystem;
    private readonly TargetFramework _targetFramework;

    public SilverlightResolverStrategy(
        IEnvironment environment,
        IFileSystem fileSystem,
        TargetFramework targetFramework)
    {
        _environment = environment;
        _fileSystem = fileSystem;
        _targetFramework = targetFramework;
    }

    public bool Test(AssemblyIdentity identity, TargetFramework? targetFramework) =>
        targetFramework?.Runtime is NetRuntime.Silverlight && _environment.OSVersion.Platform is PlatformID.Win32NT;

    public AssemblyResolverResult? Resolve(AssemblyIdentity identity)
    {
        var path = ResolveSilverlight(identity, _targetFramework.Version);
        return path != null ? new AssemblyResolverResult(AssemblySource.Gac, path) : null;
    }

    private string? ResolveSilverlight(AssemblyIdentity identity, Version version)
    {
        string[] targetFrameworkSearchPaths =
        {
            _environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            _environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
        };

        foreach (var baseDirectory in targetFrameworkSearchPaths)
        {
            var directory = Path.Combine(baseDirectory, "Microsoft Silverlight");
            if (!_fileSystem.Directory.Exists(directory))
            {
                continue;
            }

            var versionDirectory = Path.Combine(directory,
                ResolverHelper.FindClosestVersionDirectory(_fileSystem, directory, version));
            var file = ResolverHelper.SearchInDirectory(_fileSystem, identity, versionDirectory);
            if (file != null)
            {
                return file;
            }
        }

        return null;
    }
}