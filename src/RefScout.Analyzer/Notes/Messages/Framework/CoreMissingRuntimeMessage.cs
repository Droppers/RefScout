using System.Linq;
using RefScout.Analyzer.Context;
using RefScout.Analyzer.Helpers;

namespace RefScout.Analyzer.Notes.Messages.Framework;

internal class FrameworkMissingRuntimeMessage : Message<IFrameworkContext>
{
    public override NoteType Type => NoteType.MissingRuntime;

    // Add note if no runtime is found or when the desired runtime pack is not installed.
    public override bool Test(IFrameworkContext context, Assembly assembly)
        => assembly.IsEntryPoint
           && (context.EnvironmentInfo.Framework == null || context.Runtime == null);

    public override string Generate(IFrameworkContext context, Assembly assembly)
    {
        if (context.EnvironmentInfo.Framework == null)
        {
            return "Framework runtime information is unknown.";
        }

        var runtimes = context.EnvironmentInfo.Framework.Runtimes;
        var availableVersions = string.Join(", ", runtimes.Select(r => r.Version.ToMajorMinor()));
        var supportedVersions = string.Join(", ", context.SupportedRuntimes.Select(r => r.Version));

        var availableMessage = runtimes.Count == 1
            ? $"but only {availableVersions} is installed."
            : $"but only {availableVersions} are installed";

        return $"Application supports .NET Framework {supportedVersions}, {availableMessage}";
    }
}