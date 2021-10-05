using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Notes.Messages.Shared;

// Don't generate a message for assemblies, note is added whenever the loading error occurs
internal class ConfigParseErrorMessage : Message
{
    public override NoteType Type => NoteType.ConfigParseError;

    public override bool Test(IContext context, Assembly assembly) =>
        assembly.IsEntryPoint && context.Config.ErrorReport.HasErrors;

    public override string Generate(IContext context, Assembly assembly) =>
        "Error(s) occurred while trying to parse application config.";
}