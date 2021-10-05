using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.MemoryMappedFiles;
using System.Text;
using RefScout.Analyzer.Helpers;

namespace RefScout.Analyzer.Analyzers.Environment.Mono;

internal class MonoRuntimeAnalyzer : IMonoRuntimeAnalyzer
{
    private static readonly byte[] ResgenVersionPattern =
    {
        0x4D, 0x00, 0x6F, 0x00, 0x6E, 0x00, 0x6F, 0x00, 0x20, 0x00, 0x52, 0x00,
        0x65, 0x00, 0x73, 0x00, 0x6F, 0x00, 0x75, 0x00, 0x72, 0x00, 0x63, 0x00,
        0x65, 0x00, 0x20, 0x00, 0x47, 0x00, 0x65, 0x00, 0x6E, 0x00, 0x65, 0x00,
        0x72, 0x00, 0x61, 0x00, 0x74, 0x00, 0x6F, 0x00, 0x72, 0x00, 0x20, 0x00,
        0x76, 0x00, 0x65, 0x00, 0x72, 0x00, 0x73, 0x00, 0x69, 0x00, 0x6F, 0x00,
        0x6E, 0x00, 0x20, 0x00
    };

    public static readonly string[] PossibleVersionNames =
        { "4.5", "4.0", "3.5", "3.0", "2.1", "2.0", "1.1", "1.0" };

    private readonly IEnvironment _environment;
    private readonly IFileSystem _fileSystem;

    public MonoRuntimeAnalyzer(IEnvironment environment, IFileSystem fileSystem)
    {
        _environment = environment;
        _fileSystem = fileSystem;
    }

    public MonoRuntimeAnalyzerResult Analyze()
    {
        var runtimes = new List<MonoRuntime>();

        var prefixDirectories = GetRuntimePrefixDirectories();
        foreach (var prefixDirectory in prefixDirectories)
        {
            var monoLibPath = Path.Combine(prefixDirectory, "lib", "mono");
            Version? version = null;
            var frameworkVersions = new List<Version>();

            foreach (var versionName in PossibleVersionNames)
            {
                var versionPath = Path.Combine(monoLibPath, versionName);
                var resgenFileName = Path.Combine(versionPath, "resgen.exe");
                if (!_fileSystem.File.Exists(Path.Combine(resgenFileName)))
                {
                    continue;
                }

                version ??= DetermineMonoVersionFromResgen(resgenFileName);
                frameworkVersions.Add(Version.Parse(versionName));
            }


            if (frameworkVersions.Count > 0)
            {
                runtimes.Add(new MonoRuntime(version, prefixDirectory, frameworkVersions));
            }
        }

        return new MonoRuntimeAnalyzerResult(runtimes);
    }

    public IEnumerable<string> GetRuntimePrefixDirectories() => GetPrefixDirectories("MONO_PATH");

    public IEnumerable<string> GetGacPrefixDirectories() => GetPrefixDirectories("MONO_GAC_PREFIX");

    // Scans resgen.exe for a pattern to reliably determine the mono version
    public static Version? DetermineMonoVersionFromResgen(string fileName)
    {
        const char lineFeed = (char)10; // Modern mono versions
        const char space = (char)32; // Old mono versions

        using var memoryMappedFile =
            MemoryMappedFile.CreateFromFile(fileName, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        using var accessor =
            memoryMappedFile.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);

        var offset = BinaryKmpSearch.SearchInFile(accessor, ResgenVersionPattern);
        if (offset == -1)
        {
            return null;
        }

        var versionOffset = offset + ResgenVersionPattern.Length;
        var sb = new StringBuilder();
        while (true)
        {
            var asciiChar = (char)accessor.ReadInt16(versionOffset);
            if (asciiChar is lineFeed or space)
            {
                break;
            }

            sb.Append(asciiChar);
            versionOffset += 2;
        }

        return Version.TryParse(sb.ToString(), out var version) ? version : null;
    }

    private IEnumerable<string> GetPrefixDirectories(string environmentVariable)
    {
        const char monoPathSeparator = ':';

        var monoPathVariable = _environment.GetEnvironmentVariable(environmentVariable);
        if (!string.IsNullOrEmpty(monoPathVariable))
        {
            foreach (var path in monoPathVariable.Split(monoPathSeparator))
            {
                yield return path;
            }

            yield break;
        }

        // Should probably iterate path variable to find the wanted location, but who uses Mono -> not important
        switch (_environment.OSVersion.Platform)
        {
            // TODO: this currently only modern Mono installations, older versions use folder names such as 'Mono-2.0', 'Mono-3.2.3'
            case PlatformID.Win32NT:
                yield return Path.Combine(_environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles),
                    "Mono");
                yield return Path.Combine(_environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86),
                    "Mono");
                break;
            case PlatformID.MacOSX:
                yield return "/Library/Frameworks/Mono.framework/Versions/Current/";
                break;
            case PlatformID.Unix:
                yield return "/usr/";
                break;
        }
    }
}

public record MonoRuntimeAnalyzerResult(IReadOnlyList<MonoRuntime> Runtimes);

public record MonoRuntime(
    Version? Version,
    string Path,
    IReadOnlyList<Version> FrameworkVersions);