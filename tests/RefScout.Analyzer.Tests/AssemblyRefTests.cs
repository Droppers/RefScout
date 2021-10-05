using RefScout.Analyzer.Notes;
using Xunit;

namespace RefScout.Analyzer.Tests;

public class AssemblyRefTests
{
    [Fact]
    public void Level()
    {
        var reference = AssHelp.Ref(AssHelp.Ass(), AssHelp.Ass());
        Assert.Equal(NoteLevel.Default, reference.Level);

        reference.AddNote(NoteType.BindingRedirectSuccess, "success");
        Assert.Equal(NoteLevel.Success, reference.Level);

        reference.AddNote(NoteType.LoadNotFound, "not found");
        Assert.Equal(NoteLevel.Fatal, reference.Level);

        Assert.NotEmpty(reference.Notes);
        Assert.True(reference.HasNotes);
    }
}