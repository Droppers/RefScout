using System;

namespace RefScout.Analyzer.Helpers;

internal class EnvironmentWrapper : IEnvironment
{
    public bool Is64BitOperatingSystem => Environment.Is64BitOperatingSystem;
    public OperatingSystem OSVersion => Environment.OSVersion;
    public string GetFolderPath(Environment.SpecialFolder folder) => Environment.GetFolderPath(folder);
    public string? GetEnvironmentVariable(string variable) => Environment.GetEnvironmentVariable(variable);
}