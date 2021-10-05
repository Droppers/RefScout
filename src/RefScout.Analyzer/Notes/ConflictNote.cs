namespace RefScout.Analyzer.Notes;

public record ConflictNote(NoteType Type, string Message)
{
    public NoteLevel Level => (NoteLevel)(int)Type;
}