using System;

namespace RefScout.Analyzer.Helpers;

internal static class Extensions
{
    public static Version ToMajorMinor(this Version version) => new(version.Major, version.Minor);
}