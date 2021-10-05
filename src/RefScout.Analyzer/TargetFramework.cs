using System;
using RefScout.Analyzer.Helpers;

namespace RefScout.Analyzer;

public class TargetFramework
{
    private const string NetFramework = ".NETFramework";
    private const string NetPortable = ".NETPortable";
    private const string NetCoreApp = ".NETCoreApp";
    private const string NetCore = ".NETCore";
    private const string Netstandard = ".NETStandard";
    private const string Silverlight = "Silverlight";

    public TargetFramework(NetRuntime runtime, Version version)
    {
        // Trim last zero if predecessor is also a zero
        // 5.0.0 -> 5.0
        if (version.Minor == 0 && version.Build == 0)
        {
            version = version.ToMajorMinor();
        }

        Runtime = runtime;
        Version = version;
    }

    public NetRuntime Runtime { get; }
    public Version Version { get; }

    // .NET 5.0 is still called .NETCoreApp too
    public string Id => Runtime == NetRuntime.Core
        ? $"{NetCoreApp},Version=v{Version}"
        : $".NET{Runtime},Version=v{Version}";

    public string ShortName => Runtime is NetRuntime.Core or NetRuntime.Standard && Version.Major >= 5
        ? Version.ToString()
        : $"{Runtime} {Version}";

    public override string ToString() => ".NET " + ShortName;

    public static TargetFramework Parse(ReadOnlySpan<char> identifier)
    {
        const string versionProperty = ",Version=";
        var separator = identifier.IndexOf(',');
        var runtime = identifier[..separator];
        var properties = identifier[(separator + 1)..];
        var version = properties[(properties.IndexOf(versionProperty) + versionProperty.Length)..];

        var nextPropertyStart = version.IndexOf(',');
        if (nextPropertyStart != -1)
        {
            version = version[..nextPropertyStart];
        }

        if (version[0] == 'v')
        {
            version = version[1..];
        }

        var parsedVersion = Version.Parse(version);
        NetRuntime netRuntime;
        if (runtime.Equals(NetFramework, StringComparison.OrdinalIgnoreCase) ||
            runtime.Equals(NetPortable, StringComparison.OrdinalIgnoreCase))
        {
            netRuntime = parsedVersion.Major >= 5 ? NetRuntime.Silverlight : NetRuntime.Framework;
        }
        else if (runtime.Equals(NetCoreApp, StringComparison.OrdinalIgnoreCase) ||
                 runtime.Equals(NetCore, StringComparison.OrdinalIgnoreCase))
        {
            netRuntime = NetRuntime.Core;
        }
        else if (runtime.Equals(Netstandard, StringComparison.OrdinalIgnoreCase))
        {
            netRuntime = NetRuntime.Standard;
        }
        else if (runtime.Equals(Silverlight, StringComparison.OrdinalIgnoreCase))
        {
            netRuntime = NetRuntime.Silverlight;
        }
        else
        {
            throw new Exception($"Unsupported runtime in target framework: {runtime.ToString()}");
        }

        return new TargetFramework(netRuntime, parsedVersion);
    }

    public override bool Equals(object? obj)
    {
        if (obj is TargetFramework other)
        {
            return Runtime == other.Runtime && Version.Equals(other.Version);
        }

        return false;
    }

    public override int GetHashCode() => HashCode.Combine((int)Runtime, Version);
}

public enum NetRuntime
{
    Standard,
    Core,
    Framework,
    Silverlight
}