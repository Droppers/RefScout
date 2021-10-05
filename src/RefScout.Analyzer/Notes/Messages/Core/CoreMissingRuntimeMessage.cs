using System;
using System.Linq;
using RefScout.Analyzer.Analyzers.Environment.Core;
using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Notes.Messages.Core;

internal class CoreMissingRuntimeMessage : Message<ICoreContext>
{
    public override NoteType Type => NoteType.MissingRuntime;

    // Add note if no runtime is found or when the desired runtime pack is not installed.
    public override bool Test(ICoreContext context, Assembly assembly) =>
        assembly.IsEntryPoint
        && !context.Config.SelfContained
        && (context.Runtime == null || !context.Runtime.Packs.Contains(context.Config.RuntimePack) ||
            !context.Runtime.Packs.Contains(RuntimePack.Default));

    public override string Generate(ICoreContext context, Assembly assembly)
    {
        var runtimeVersion = context.Config.TargetRuntimeVersion ?? assembly.TargetFramework?.Version;

        if (context.EnvironmentInfo.Core == null)
        {
            return "Core runtime information is unknown.";
        }

        if (runtimeVersion == null)
        {
            return
                "Target runtime version of this assembly could not be determined, therefore runtime assemblies could not be resolved.";
        }

        if (context.Runtime == null)
        {
            if (context.EnvironmentInfo == null)
            {
                throw new Exception("EnvironmentInfo on context cannot be null.");
            }

            var availableVersions =
                string.Join(", ", context.EnvironmentInfo.Core.Runtimes.Select(r => r.VersionName));
            return
                $"Runtime version {runtimeVersion} is not installed, installed versions are: {availableVersions}.";
        }

        var packs = string.Join(", ", context.Runtime.Packs);
        var missingPack = !context.Runtime.Packs.Contains(RuntimePack.Default)
            ? RuntimePack.Default
            : context.Config.RuntimePack;
        return
            $"The .NET Core {context.Runtime.VersionName} runtime framework {missingPack} is required but only {packs} {(packs.Length > 1 ? "are" : "is")} installed.";
    }
}