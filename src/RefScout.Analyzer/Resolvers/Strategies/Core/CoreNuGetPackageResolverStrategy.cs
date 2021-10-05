using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using RefScout.Analyzer.Config.Core;
using RefScout.Analyzer.Helpers;

namespace RefScout.Analyzer.Resolvers.Strategies.Core;

// This code is not pretty and should be simplified, wrapping the deps file information in
// another object is very pointless
internal class CoreNuGetPackageResolverStrategy : IResolverStrategy
{
    private readonly IEnvironment _environment;
    private readonly IFileSystem _fileSystem;

    private readonly List<string> _searchDirectories;
    private readonly TargetFramework _targetFramework;
    private readonly DepsFile _depsFile;

    private bool _initialized;

    public CoreNuGetPackageResolverStrategy(
        IEnvironment environment,
        IFileSystem fileSystem,
        TargetFramework targetFramework,
        DepsFile depsFile)
    {
        _environment = environment;
        _fileSystem = fileSystem;
        _targetFramework = targetFramework;
        _depsFile = depsFile;
        _searchDirectories = new List<string>();
    }

    public bool Test(AssemblyIdentity identity, TargetFramework? targetFramework) =>
        targetFramework?.Runtime is NetRuntime.Core or NetRuntime.Standard;

    public AssemblyResolverResult? Resolve(AssemblyIdentity identity)
    {
        // Initializing is a 'heavy' task, so only do when necessary
        if (!_initialized)
        {
            Initialize();
        }

        foreach (var basePath in _searchDirectories)
        {
            if (_fileSystem.File.Exists(Path.Combine(basePath, identity.Name + ".dll")))
            {
                return new AssemblyResolverResult(AssemblySource.Local,
                    Path.Combine(basePath, identity.Name + ".dll"));
            }

            if (_fileSystem.File.Exists(Path.Combine(basePath, identity.Name + ".exe")))
            {
                return new AssemblyResolverResult(AssemblySource.Local,
                    Path.Combine(basePath, identity.Name + ".exe"));
            }
        }

        return null;
    }

    private void Initialize()
    {
        _initialized = true;

        var packages = LoadPackageInfos(_targetFramework.Id).ToArray();
        foreach (var path in GetLookupPaths())
        {
            foreach (var p in packages)
            {
                foreach (var item in p.RuntimeComponents)
                {
                    var itemPath = _fileSystem.Path.GetDirectoryName(item);

                    if (itemPath == null)
                    {
                        continue;
                    }

                    var fullPath = Path.Combine(path, p.Name, p.Version, itemPath).ToLowerInvariant();
                    if (_fileSystem.Directory.Exists(fullPath))
                    {
                        _searchDirectories.Add(fullPath);
                    }
                }
            }
        }
    }

    private IEnumerable<string> GetLookupPaths()
    {
        yield return Path.Combine(_environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget",
            "packages");
    }

    private IEnumerable<DotNetCorePackageInfo> LoadPackageInfos(
        string targetFramework)
    {
        if (_depsFile.Libraries == null)
        {
            yield break;
        }

        var runtimeInfos = _depsFile.Targets?.FirstOrDefault(target => target.Key
            .StartsWith(targetFramework, StringComparison.OrdinalIgnoreCase) && target.Value.Count > 0).Value;
        if (runtimeInfos == null)
        {
            yield break;
        }

        foreach (var (key, _) in _depsFile.Libraries)
        {
            if (!runtimeInfos.ContainsKey(key))
            {
                continue;
            }

            var runtimeInfo = runtimeInfos[key].Runtime;
            var components = new string[runtimeInfo?.Count ?? 0];
            if (runtimeInfo != null)
            {
                var i = 0;
                foreach (var component in runtimeInfo)
                {
                    components[i] = component.Key;
                    i++;
                }
            }

            yield return new DotNetCorePackageInfo(key, components);
        }
    }

    private class DotNetCorePackageInfo
    {
        public DotNetCorePackageInfo(string fullName, string[] runtimeComponents)
        {
            var parts = fullName.Split('/');
            Name = parts[0];
            Version = parts.Length > 1 ? parts[1] : "<UNKNOWN>";

            RuntimeComponents = runtimeComponents;
        }

        public string Name { get; }
        public string Version { get; }
        public string[] RuntimeComponents { get; }
    }
}