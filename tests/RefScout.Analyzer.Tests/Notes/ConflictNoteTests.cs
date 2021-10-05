using RefScout.Analyzer.Notes;
using Xunit;

namespace RefScout.Analyzer.Tests.Notes;

public class ConflictNoteTests
{
    [Fact]
    public void Level()
    {
        var note = new ConflictNote(NoteType.ArchitectureMismatch, "test");
        Assert.Equal(NoteLevel.Warning, note.Level);
    }
}