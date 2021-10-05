using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using RefScout.Analyzer.Config.Core;
using RefScout.Analyzer.Helpers;
using RefScout.Core.Logging;

namespace RefScout.Analyzer.Analyzers.Environment.Core;

internal class CoreRuntimeAnalyzer : ICoreRuntimeAnalyzer
{
    private const string PackWindowsDesktop = "Microsoft.WindowsDesktop.App";
    private const string PackAspNetCore = "Microsoft.AspNetCore.App";
    private const string PackAspNetCoreAll = "Microsoft.AspNetCore.All";
    private const string PackDefault = "Microsoft.NETCore.App";

    public static readonly string[] Packs =
    {
        PackDefault,
        PackWindowsDesktop,
        PackAspNetCore,
        PackAspNetCoreAll
    };

    public static readonly IReadOnlyDictionary<string, RuntimePack> StringToPack =
        new Dictionary<string, RuntimePack>
        {
            { PackDefault, RuntimePack.Default },
            { PackWindowsDesktop, RuntimePack.WindowsDesktop },
            { PackAspNetCore, RuntimePack.AspNetCore },
            { PackAspNetCoreAll, RuntimePack.AspNetCoreAll }
        };

    public static readonly IReadOnlyDictionary<RuntimePack, string> PackToString =
        new Dictionary<RuntimePack, string>
        {
            { RuntimePack.Default, PackDefault },
            { RuntimePack.WindowsDesktop, PackWindowsDesktop },
            { RuntimePack.AspNetCore, PackAspNetCore },
            { RuntimePack.AspNetCoreAll, PackAspNetCoreAll }
        };


    private readonly IEnvironment _environment;
    private readonly IFileSystem _fileSystem;

    private readonly Lazy<IReadOnlyList<(bool is64Bit, string path)>> _dotnetLocations;

    public CoreRuntimeAnalyzer(IEnvironment environment, IFileSystem fileSystem)
    {
        _environment = environment;
        _fileSystem = fileSystem;
        _dotnetLocations =
            new Lazy<IReadOnlyList<(bool is64Bit, string path)>>(() => InitDotNetLocations().ToList());
    }

    public CoreRuntime? FindRuntime(
        Version requestedVersion,
        RollForwardBehavior behavior = RollForwardBehavior.Minor,
        bool? is64Bit = null)
    {
        var runtimeVersions = LocateAll(is64Bit);

        // Environment variable overrides runtime config
        var environmentBehavior = _environment.GetEnvironmentVariable("DOTNET_ROLL_FORWARD");
        if (environmentBehavior != null &&
            Enum.TryParse(environmentBehavior, out RollForwardBehavior parsedBehavior))
        {
            behavior = parsedBehavior;
        }

        return FindRuntime(requestedVersion, runtimeVersions, behavior, is64Bit);
    }

    public CoreRuntimeAnalyzerResult Analyze() => new(LocateAll());

    public IReadOnlyList<CoreRuntime> LocateAll(bool? is64Bit = null)
    {
        var allVersions = new HashSet<CoreRuntime>();

        foreach (var location in _dotnetLocations.Value)
        {
            if (is64Bit != null && location.is64Bit != is64Bit)
            {
                Logger.Info($"Skipping {(is64Bit == true ? "32" : "64")}-bit runtime location: {location.path}");
                continue;
            }

            var sharedPath = Path.Combine(location.path, "shared");
            if (!_fileSystem.Directory.Exists(sharedPath))
            {
                continue;
            }

            foreach (var pack in Packs)
            {
                var packPath = Path.Combine(sharedPath, pack);
                if (!_fileSystem.Directory.Exists(packPath))
                {
                    continue;
                }

                var versions = _fileSystem.Directory.GetDirectories(packPath)
                    .Select(d => ConvertNameToRuntime(Path.GetFileName(d), sharedPath, location.is64Bit))
                    .OfType<CoreRuntime>();

                foreach (var version in versions)
                {
                    var existingVersion = allVersions.FirstOrDefault(v => v.Equals(version));
                    if (existingVersion != null)
                    {
                        existingVersion.Packs.Add(StringToPack[pack]);
                    }
                    else
                    {
                        version.Packs.Add(StringToPack[pack]);
                        allVersions.Add(version);
                    }
                }
            }
        }

        return allVersions.OrderBy(x => x.Version).ThenBy(x => x.Preview).ToList();
    }

    // Based on information found at, might not be identical due to vague wording in some parts:
    // https://github.com/dotnet/runtime/blob/main/docs/design/features/framework-version-resolution.md#roll-forward
    public CoreRuntime? FindRuntime(
        Version requestedVersion,
        IEnumerable<CoreRuntime> availableRuntimes,
        RollForwardBehavior behavior,
        bool? is64Bit = null)
    {
        var runtimes = availableRuntimes.Where(x => is64Bit == null || x.Is64Bit == is64Bit).ToArray();
        if (behavior == RollForwardBehavior.LatestPatch)
        {
            return runtimes.Where(v =>
                    v.Version.Major == requestedVersion.Major && v.Version.Minor == requestedVersion.Minor)
                .OrderByDescending(v => v.Version.Build).FirstOrDefault();
        }

        if (behavior == RollForwardBehavior.Minor)
        {
            if (runtimes.Any(v =>
                v.Version.Major == requestedVersion.Major && v.Version.Minor == requestedVersion.Minor))
            {
                return FindRuntime(requestedVersion, runtimes, RollForwardBehavior.LatestPatch);
            }

            return runtimes
                .Where(v => v.Version.Major == requestedVersion.Major &&
                            v.Version.Minor > requestedVersion.Minor)
                .OrderBy(v => v.Version.Minor).ThenByDescending(v => v.Version.Build).FirstOrDefault();
        }

        if (behavior == RollForwardBehavior.Major)
        {
            if (runtimes.Any(v =>
                v.Version.Major == requestedVersion.Major && v.Version.Minor >= requestedVersion.Minor))
            {
                return FindRuntime(requestedVersion, runtimes, RollForwardBehavior.Minor);
            }

            return runtimes
                .Where(v => v.Version.Major > requestedVersion.Major)
                .OrderBy(v => v.Version.Major)
                .ThenBy(v => v.Version.Minor)
                .ThenByDescending(v => v.Version.Build)
                .FirstOrDefault();
        }

        if (behavior == RollForwardBehavior.LatestMinor)
        {
            return runtimes
                .Where(v => v.Version.Major == requestedVersion.Major)
                .OrderByDescending(v => v.Version.Minor)
                .ThenByDescending(v => v.Version.Build)
                .FirstOrDefault();
        }

        if (behavior == RollForwardBehavior.LatestMajor)
        {
            return runtimes.OrderByDescending(v => v.Version.Major)
                .ThenByDescending(v => v.Version.Minor)
                .ThenByDescending(v => v.Version.Build)
                .FirstOrDefault();
        }

        return runtimes.FirstOrDefault(v => v.Version == requestedVersion);
    }

    private static CoreRuntime? ConvertNameToRuntime(string name, string path, bool is64Bit)
    {
        try
        {
            var shortName = name;
            var dashIndex = shortName.IndexOf('-');
            if (dashIndex > 0)
            {
                shortName = shortName.Remove(dashIndex);
            }

            var previewIndex = Math.Max(name.IndexOf("preview.", StringComparison.Ordinal),
                name.IndexOf("rc.", StringComparison.Ordinal));
            var previewVersion = -1;
            var previewBuildVersion = -1;
            if (previewIndex > 0)
            {
                var split = name[previewIndex..].Split('.');
                previewVersion = split.Length >= 1 ? int.Parse(split[1]) : previewVersion;
                previewBuildVersion = split.Length >= 2 ? int.Parse(split[2]) : previewBuildVersion;
            }

            if (Version.TryParse(shortName, out var parsedVersion))
            {
                return new CoreRuntime(parsedVersion, previewVersion, previewBuildVersion, name, path,
                    is64Bit);
            }

            Logger.Warn($"Invalid version format: {shortName}.");
            return null;
        }
        catch
        {
            Logger.Warn($"Could not parse runtime version name: {name} in {path}.");
            return null;
        }
    }

    private IEnumerable<(bool is64Bit, string path)> InitDotNetLocations()
    {
        // User can configure a custom dotnet root where dotnet is located, which is then prioritized
        var dotnetRootX64 = _environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (!string.IsNullOrWhiteSpace(dotnetRootX64))
        {
            yield return (true, dotnetRootX64);
        }

        var dotnetRootX86 = _environment.GetEnvironmentVariable("DOTNET_ROOT(x86)");
        if (!string.IsNullOrWhiteSpace(dotnetRootX86))
        {
            yield return (false, dotnetRootX86);
        }

        // TODO: This is not correct
        if (!string.IsNullOrWhiteSpace(dotnetRootX64) || !string.IsNullOrWhiteSpace(dotnetRootX86))
        {
            yield break;
        }

        switch (_environment.OSVersion.Platform)
        {
            case PlatformID.Unix:
                yield return (true, "/usr/share/dotnet/");
                break;
            case PlatformID.MacOSX:
                yield return (true, "/usr/local/share/dotnet/");
                break;
            case PlatformID.Win32NT:
                yield return (true, Path.Combine(
                    _environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles),
                    "dotnet"));
                yield return (false, Path.Combine(
                    _environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86),
                    "dotnet"));
                break;
            default:
                Logger.Warn("This platform is not supported by the .NET Core runtime analyzer.");
                break;
        }
    }
}