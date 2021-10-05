using System;

namespace RefScout.Analyzer.Helpers;

internal interface IEnvironment
{
    bool Is64BitOperatingSystem { get; }

    // ReSharper disable once InconsistentNaming
    OperatingSystem OSVersion { get; }

    string GetFolderPath(Environment.SpecialFolder folder);

    string? GetEnvironmentVariable(string variable);
}