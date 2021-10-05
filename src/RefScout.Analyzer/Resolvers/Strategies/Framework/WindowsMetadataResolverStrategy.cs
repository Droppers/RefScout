using System;
using System.IO;
using System.IO.Abstractions;
using RefScout.Analyzer.Helpers;

namespace RefScout.Analyzer.Resolvers.Strategies.Framework;

internal class WindowsMetadataResolverStrategy : IResolverStrategy
{
    private readonly IEnvironment _environment;
    private readonly IFileSystem _fileSystem;

    public WindowsMetadataResolverStrategy(IEnvironment environment, IFileSystem fileSystem)
    {
        _environment = environment;
        _fileSystem = fileSystem;
    }

    public bool Test(AssemblyIdentity identity, TargetFramework? targetFramework) =>
        identity.IsWindowsRuntime && _environment.OSVersion.Platform is PlatformID.Win32NT;

    public AssemblyResolverResult? Resolve(AssemblyIdentity identity)
    {
        var path = FindWindowsMetadataFile(identity);
        return path != null ? new AssemblyResolverResult(AssemblySource.Gac, path) : null;
    }

    private string? FindWindowsMetadataFile(AssemblyIdentity identity)
    {
        // TODO: this could technically be read from the registry: "HKLM\SOFTWARE\Wow6432Node\Microsoft\Windows Kits\Installed Roots"
        var basePath = Path.Combine(_environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Windows Kits", "10", "References");

        if (!_fileSystem.Directory.Exists(basePath))
        {
            return FindWindowsMetadataInSystemDirectory(identity);
        }

        // TODO : Find a way to detect the required Windows SDK version.
        var di = _fileSystem.DirectoryInfo.FromDirectoryName(basePath);
        basePath = null;
        foreach (var versionFolder in di.GetDirectories())
        {
            basePath = versionFolder.FullName;
        }

        if (basePath == null)
        {
            return FindWindowsMetadataInSystemDirectory(identity);
        }

        basePath = Path.Combine(basePath, identity.Name);
        if (!_fileSystem.Directory.Exists(basePath))
        {
            return FindWindowsMetadataInSystemDirectory(identity);
        }

        basePath = Path.Combine(basePath,
            ResolverHelper.FindClosestVersionDirectory(_fileSystem, basePath, identity.Version));
        if (!_fileSystem.Directory.Exists(basePath))
        {
            return FindWindowsMetadataInSystemDirectory(identity);
        }

        var file = Path.Combine(basePath, identity.Name + ".winmd");
        return _fileSystem.File.Exists(file)
            ? file
            : FindWindowsMetadataInSystemDirectory(identity);
    }

    private string? FindWindowsMetadataInSystemDirectory(AssemblyIdentity identity)
    {
        var file = Path.Combine(Environment.SystemDirectory, "WinMetadata", identity.Name + ".winmd");
        return _fileSystem.File.Exists(file)
            ? file
            : null;
    }
}