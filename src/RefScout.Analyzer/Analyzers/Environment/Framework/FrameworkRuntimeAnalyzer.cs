using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace RefScout.Analyzer.Analyzers.Environment.Framework;

// I am definitely not going to test the registry, this art is copied from MSDN anyways
[ExcludeFromCodeCoverage]
internal class FrameworkRuntimeAnalyzer : IFrameworkRuntimeAnalyzer
{
    [SupportedOSPlatform("windows")]
    public FrameworkRuntimeAnalyzerResult Analyze() => new(LocateRuntimes());

    [SupportedOSPlatform("windows")]
    public FrameworkRuntime? FindRuntime(IEnumerable<TargetFramework> targets)
    {
        var runtimes = LocateRuntimes().OrderByDescending(t => t.Version).ToList();

        foreach (var target in targets)
        {
            foreach (var runtime in runtimes)
            {
                var targetsClr2 = target.Version.Major == 2 && target.Version.Minor == 0;
                if (targetsClr2 && target.Version.Major == runtime.RuntimeVersion.Major &&
                    target.Version.Minor == runtime.RuntimeVersion.Minor)
                {
                    return runtime;
                }

                if (!targetsClr2 && target.Version <= runtime.Version)
                {
                    return runtime;
                }
            }
        }

        return null;
    }

    [SupportedOSPlatform("windows")]
    private static IReadOnlyList<FrameworkRuntime> LocateRuntimes()
    {
        var runtimes = GetInstalledRuntimes().ToList();
        var runtimeAfter45 = GetFrameworkVersionAfter45();
        if (runtimeAfter45 == null)
        {
            return runtimes;
        }

        // Version later than 4.5 installed, so eliminate the previously found 4.0 from registry
        runtimes.RemoveAt(runtimes.FindIndex(v => v.RuntimeVersion.Major == 4));
        runtimes.Add(runtimeAfter45);
        return runtimes;
    }

    [SupportedOSPlatform("windows")]
    private static FrameworkRuntime? GetFrameworkVersionAfter45()
    {
        using var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
            .OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\");
        return ndpKey?.GetValue("Release") != null
            ? CheckFor45PlusVersion(ndpKey.GetValue("Release") as int?)
            : null;
    }

    private static FrameworkRuntime? CheckFor45PlusVersion(int? releaseKey)
    {
        var versionString = releaseKey switch
        {
            >= 528040 => "4.8",
            >= 461808 => "4.7.2",
            >= 461308 => "4.7.1",
            >= 460798 => "4.7",
            >= 394802 => "4.6.2",
            >= 394254 => "4.6.1",
            >= 393295 => "4.6",
            >= 379893 => "4.5.2",
            >= 378675 => "4.5.1",
            >= 378389 => "4.5",
            _ => null
        };

        return versionString != null
            ? new FrameworkRuntime(GetRuntimeVersion("v4.0"), Version.Parse(versionString))
            : null;
    }

    [SupportedOSPlatform("windows")]
    private static IEnumerable<FrameworkRuntime> GetInstalledRuntimes()
    {
        var runtimes = new List<FrameworkRuntime>();

        using var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
            .OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\");
        if (ndpKey == null)
        {
            return runtimes;
        }

        foreach (var versionKeyName in ndpKey.GetSubKeyNames())
        {
            // Skip .NET Framework 4.5 version information.
            if (versionKeyName == "v4" || !versionKeyName.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var versionKey = ndpKey.OpenSubKey(versionKeyName);
            if (versionKey == null)
            {
                continue;
            }

            var version = versionKey.GetValue("Version", "") as string;
            var sp = versionKey.GetValue("SP", "")?.ToString();
            var install = versionKey.GetValue("Install", "")?.ToString();

            // 1 = installed
            if (install == "1" && !string.IsNullOrEmpty(version))
            {
                _ = int.TryParse(sp, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var servicePack);
                runtimes.Add(new FrameworkRuntime(GetRuntimeVersion(versionKeyName),
                    Version.Parse(version), servicePack));
            }

            if (!string.IsNullOrEmpty(version))
            {
                continue;
            }

            foreach (var subKeyName in versionKey.GetSubKeyNames())
            {
                var subKey = versionKey.OpenSubKey(subKeyName);
                var subVersion = subKey?.GetValue("Version", "") as string;
                if (subKey == null || string.IsNullOrEmpty(subVersion))
                {
                    continue;
                }

                sp = subKey.GetValue("SP", "")?.ToString();
                install = subKey.GetValue("Install", "")?.ToString();
                if (install != "1")
                {
                    continue;
                }

                _ = int.TryParse(sp, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var servicePack);
                runtimes.Add(new FrameworkRuntime(GetRuntimeVersion(versionKeyName),
                    Version.Parse(subVersion), servicePack));
            }
        }

        return runtimes;
    }

    private static Version GetRuntimeVersion(string versionString)
    {
        var version = Version.Parse(versionString.TrimStart('v'));
        return version.Major switch
        {
            1 when version.Minor == 0 => new Version(1, 0, 3705),
            1 when version.Minor == 1 => new Version(1, 1, 4322),
            >= 2 and <= 3 => new Version(2, 0, 50727),
            _ => new Version(4, 0, 30319)
        };
    }
}