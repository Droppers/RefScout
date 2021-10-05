using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using RefScout.Core;

namespace RefScout.Wpf.Helpers;

internal static class GraphVizHelper
{
    private const string GraphVizBin = @"Graphviz\bin";
    private const string GraphVizExecutable = "dot.exe";

    private const string GraphVizDownloadLocation =
        "https://gitlab.com/api/v4/projects/4207231/packages/generic/graphviz-releases/2.49.1/stable_windows_10_cmake_Release_x64_graphviz-install-2.49.1-win64.exe";

    private static readonly string AppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        Constants.ApplicationName);

    public static readonly string TempGraphVizFolder = Path.Combine(Path.GetTempPath(), "GraphViz");

    private static readonly IEnumerable<string> PossibleGraphVizLocations = new List<string>
    {
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
        AppDataFolder
    }.Select(path => Path.Combine(path, GraphVizBin));

    public static bool IsGraphVizInstalled() =>
        ProcessHelper.LocateExecutableInPath(GraphVizExecutable, PossibleGraphVizLocations) != null;

    public static async Task<string> ConvertGraphVizToImage(string graphFileName)
    {
        var pathToDot = ProcessHelper.LocateExecutableInPath(GraphVizExecutable, PossibleGraphVizLocations);
        if (pathToDot == null)
        {
            throw new FileNotFoundException("Could not find path to GraphViz dot.exe");
        }

        var outputFileName = Path.Combine(TempGraphVizFolder, Path.GetRandomFileName());
        var exitCode = await ProcessHelper.RunProcessAsync(pathToDot, $"-Tsvg {graphFileName} -o {outputFileName}")
            .ConfigureAwait(false);
        if (exitCode != 0)
        {
            throw new FileLoadException($"GraphViz failed with exit code {exitCode}");
        }

        // Insert fake overlay rectangles for better hit testing in SharpVectors
        await InsertHitTestRectangles(outputFileName);

        return outputFileName;
    }

    private static async Task InsertHitTestRectangles(string fileName)
    {
        var contents = await File.ReadAllTextAsync(fileName);
        var sb = new StringBuilder();
        var nodes = Regex.Matches(contents, @"<g id=""([\w_,.\s]*?)"" class=""node"">.*?<path.*?d=""(.*?)"".*?<\/g>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        foreach (Match node in nodes)
        {
            var id = node.Groups[1].Value;
            var path = node.Groups[2].Value;

            var geometry = Geometry.Parse(path);
            var bounds = geometry.Bounds;
            sb.Append("<rect id=\"hit-")
                .Append(id).Append("\" x=\"")
                .Append(bounds.Left)
                .Append("\" y=\"")
                .Append(bounds.Top)
                .Append("\" width=\"")
                .Append(bounds.Width)
                .Append("\" height=\"")
                .Append(bounds.Height)
                .AppendLine("\" style=\"fill:rgba(0,0,0,0);\" />");
        }

        contents = contents.Insert(contents.Length - 14, sb.ToString());
        await File.WriteAllTextAsync(fileName, contents);
    }

    public static async Task DownloadAndInstallGraphViz(IProgress<string>? progress = null)
    {
        progress?.Report("Downloading");
        var fileName = Path.Combine(AppDataFolder, "graphviz-install-2.49.1-win64.exe");

        // Download GraphViz
        using var client = new HttpClient();
        using var response = await client.GetAsync(new Uri(GraphVizDownloadLocation));
        await using var fs = new FileStream(fileName, FileMode.OpenOrCreate);
        await response.Content.CopyToAsync(fs);
        await fs.DisposeAsync();

        await Task.Run(() =>
        {
            try
            {
                progress?.Report("Installing");
                RunInstaller(fileName, Path.Combine(AppDataFolder, "GraphViz"));
            }
            finally
            {
                progress?.Report("Removing temporary files");
                File.Delete(fileName);
            }
        });
    }

    private static void RunInstaller(string installerFileName, string outputDirectory)
    {
        const int cancelledErrorCode = 1223;

        var psi = new ProcessStartInfo(installerFileName)
        {
            Arguments = $@"/S /D={outputDirectory}",
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            UseShellExecute = true,
            Verb = "runas"
        };

        try
        {
            using var process = Process.Start(psi);
            process?.WaitForExit();

            var exitCode = process?.HasExited == true ? process.ExitCode : -1;
            if (exitCode != 0)
            {
                throw new Exception($"GraphViz installation failed with exit code {exitCode}");
            }

            if (!Directory.Exists(outputDirectory))
            {
                throw new Exception("GraphViz installation failed, directory does not exist.");
            }
        }
        catch (Win32Exception e) when (e.NativeErrorCode == cancelledErrorCode)
        {
            throw new Exception("GraphViz installation was cancelled by the user.");
        }
    }
}