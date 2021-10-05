using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace RefScout.Core.Helpers;

public static class ResourceHelper
{
    public static bool ExtractResourceToFile<T>(
        IFileSystem fileSystem,
        string embeddedFileName,
        string destinationFileName)
    {
        var resourceName = GetResourceName<T>(embeddedFileName);
        if (resourceName == null)
        {
            return false;
        }

        using var resourceToSave = typeof(T).Assembly.GetManifestResourceStream(resourceName);

        var destinationPath = Path.GetDirectoryName(destinationFileName);
        fileSystem.Directory.CreateDirectory(destinationPath!);
        using var output = fileSystem.File.Open(destinationFileName, FileMode.Create);
        resourceToSave?.CopyTo(output);
        return true;
    }

    private static string? GetResourceName<T>(string fileName) =>
        typeof(T).Assembly.GetManifestResourceNames().FirstOrDefault(resourceName =>
            resourceName.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
}