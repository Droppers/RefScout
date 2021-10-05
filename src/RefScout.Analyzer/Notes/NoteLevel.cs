namespace RefScout.Analyzer.Notes;

public enum NoteLevel
{
    Message = 1, // A grayed out level, don't display if it will clutter the results
    Default = 2,
    Success = 4,
    Info = 8,
    Warning = 16,
    Fatal = 32
}