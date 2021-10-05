using System;
using RefScout.Analyzer.Helpers;

namespace RefScout.Analyzer.Tests;

public class FakeEnvironment : IEnvironment
{
    public bool Is64BitOperatingSystem { get; set; } = true;
    public OperatingSystem OSVersion { get; set; } = new(PlatformID.Win32NT, Version.Parse("10.0"));

    public string GetFolderPath(Environment.SpecialFolder folder) => folder switch
    {
        Environment.SpecialFolder.Windows => @"C:\WINDOWS",
        Environment.SpecialFolder.ProgramFiles => @"C:\Program Files",
        Environment.SpecialFolder.ProgramFilesX86 => @"C:\Program Files (x86)",
        Environment.SpecialFolder.UserProfile => @"C:\Users\Joery",
        Environment.SpecialFolder.LocalApplicationData => @"C:\Users\Joery\AppData\Local",
        _ => throw new ArgumentOutOfRangeException(nameof(folder),
            $"Fake environment does not have a definition for this special folder: {folder}")
    };

    public string? GetEnvironmentVariable(string variable) => variable switch
    {
        "DOTNET_ROOT" => @"C:\Program Files\dotnet",
        "DOTNET_ROOT(x86)" => "",
        "DOTNET_ROLL_FORWARD" => "",
        "MONO_PATH" => null,
        "MONO_GAC_PREFIX" => @"/usr:/test",
        _ => throw new ArgumentOutOfRangeException(nameof(variable),
            $"Fake environment does not contain this environment variable: {variable}.")
    };
}