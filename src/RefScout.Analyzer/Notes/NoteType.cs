using System.Diagnostics.CodeAnalysis;

namespace RefScout.Analyzer.Notes;

[SuppressMessage("Design", "CA1069:Enums values should not be duplicated",
    Justification = "Types can have the same warning level")]
public enum NoteType
{
    Unreferenced = NoteLevel.Message,
    Unification = NoteLevel.Default,

    ConfigParseError = NoteLevel.Fatal,

    VersionMismatch = NoteLevel.Info,
    VersionMismatchWarning = NoteLevel.Warning,
    VersionMismatchFatal = NoteLevel.Fatal,

    ArchitectureMismatch = NoteLevel.Warning,

    BindingRedirectFailed = NoteLevel.Fatal,
    BindingRedirectSuccess = NoteLevel.Success,

    MissingRuntime = NoteLevel.Fatal,

    LoadNotFound = NoteLevel.Fatal,
    LoadError = NoteLevel.Fatal
}