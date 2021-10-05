using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RefScout.Wpf.Helpers;

internal static class ProcessHelper
{
    public static void LaunchBrowser(string url)
    {
        Process.Start(new ProcessStartInfo(url)
        {
            UseShellExecute = true
        });
    }

    public static string? LocateExecutableInPath(string executable, IEnumerable<string> possiblePaths)
    {
        var environmentPath = Environment.GetEnvironmentVariable("PATH");
        var paths = environmentPath?.Split(Path.PathSeparator).ToList() ?? new List<string>();
        paths.InsertRange(0, possiblePaths);
        return paths.Select(x => Path.Combine(x, executable))
            .FirstOrDefault(File.Exists);
    }

    public static async Task<int> RunProcessAsync(string fileName, string args)
    {
        using var process = new Process
        {
            StartInfo =
            {
                FileName = fileName, Arguments = args,
                UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true
            },
            EnableRaisingEvents = true
        };
        return await RunProcessAsync(process);
    }

    private static Task<int> RunProcessAsync(Process process)
    {
        var tcs = new TaskCompletionSource<int>();
        process.Exited += (_, _) => tcs.SetResult(process.ExitCode);

        var started = process.Start();
        if (!started)
        {
            throw new InvalidOperationException($"Could not start process: {process}");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return tcs.Task;
    }
}