using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using RefScout.Core.Logging;

namespace RefScout.Analyzer.Resolvers.Strategies;

public static class ResolverHelper
{
    private static (Version? version, string? directoryName) ConvertNameToVersion(string name)
    {
        try
        {
            var shortName = name;
            var dashIndex = shortName.IndexOf('-');
            if (dashIndex > 0)
            {
                shortName = shortName.Remove(dashIndex);
            }

            if (!Version.TryParse(shortName, out var parsedVersion))
            {
                Logger.Warn($"Could not convert directory version name to version: {name}");
            }

            return (parsedVersion, name);
        }
        catch
        {
            Logger.Warn($"Could not convert directory version name to version: {name}");
            return (null, null);
        }
    }

    public static string FindClosestVersionDirectory(IFileSystem fileSystem, string? basePath, Version? version)
    {
        if (basePath == null)
        {
            return ".";
        }

        string? path = null;
        foreach (var (directoryVersion, directoryName) in fileSystem.DirectoryInfo.FromDirectoryName(basePath)
            .GetDirectories()
            .Select(d => ConvertNameToVersion(d.Name))
            .Where(v => v.version != null)
            .OrderByDescending(v => v.version))
        {
            if (path == null || version == null || directoryVersion >= version)
            {
                path = directoryName;
            }
        }

        return path ?? version?.ToString() ?? ".";
    }

    public static string? SearchInDirectory(IFileSystem fileSystem, AssemblyIdentity identity, string directory)
    {
        var extensions = identity.IsWindowsRuntime ? new[] { ".winmd", ".dll" } : new[] { ".dll", ".exe" };

        // As per documentation, only look in culture specific directories if culture is specified
        // https://docs.microsoft.com/en-us/dotnet/framework/deployment/how-the-runtime-locates-assemblies#probing-the-application-base-and-culture-directories
        if (identity.Culture != AssemblyIdentity.CultureNeutral)
        {
            return extensions
                .Select(extension => Path.Combine(directory, identity.Culture, identity.Name + extension))
                .FirstOrDefault(fileSystem.File.Exists);
        }

        return extensions
            .Select(extension => Path.Combine(directory, identity.Name + extension))
            .FirstOrDefault(fileSystem.File.Exists);
    }
}