using System;
using RefScout.Analyzer.Context;
using RefScout.Analyzer.Helpers;

namespace RefScout.Analyzer.Notes.Messages.Mono;

internal class MonoMissingRuntimeMessage : Message<IMonoContext>
{
    private readonly IEnvironment _environment;

    public MonoMissingRuntimeMessage(IEnvironment environment)
    {
        _environment = environment;
    }

    public override NoteType Type => NoteType.MissingRuntime;

    // Add note if no runtime is found or when the desired runtime pack is not installed.
    public override bool Test(IMonoContext context, Assembly assembly) =>
        assembly.IsEntryPoint && context.Runtime == null;

    public override string Generate(IMonoContext context, Assembly assembly) =>
        _environment.OSVersion.Platform == PlatformID.Win32NT
            ? "No installation of Mono runtime was found, consider analyzing using the .NET Framework runtime."
            : "Mono runtime is required for running .NET Framework applications on Unix-based operating systems.";
}