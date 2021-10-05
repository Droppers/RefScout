using System;
using System.Collections.Generic;

namespace RefScout.Analyzer.Analyzers.Environment.Core;

public class CoreRuntime
{
    internal CoreRuntime(Version version, string versionName, string path) :
        this(version, -1, -1, versionName, path, true) { }

    public CoreRuntime(
        Version version,
        int preview,
        int previewBuild,
        string versionName,
        string path,
        bool is64Bit)
    {
        Version = version;
        Preview = preview;
        PreviewBuild = previewBuild;
        VersionName = versionName;
        Path = path;
        Is64Bit = is64Bit;
    }

    public Version Version { get; }
    public int Preview { get; }
    public int PreviewBuild { get; }
    public string VersionName { get; }
    public string Path { get; }
    public bool Is64Bit { get; }

    public bool IsPreview => Preview != -1;

    public HashSet<RuntimePack> Packs { get; } = new();

    public override string ToString() => $"{VersionName}{(!Is64Bit ? " (x86)" : "")}";

    public override bool Equals(object? obj)
    {
        if (obj is CoreRuntime other)
        {
            return Version == other.Version && Preview == other.Preview && PreviewBuild == other.PreviewBuild &&
                   Is64Bit == other.Is64Bit;
        }

        return false;
    }

    public override int GetHashCode() => HashCode.Combine(Version, Preview, PreviewBuild, Is64Bit);
}