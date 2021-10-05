using System;
using System.Collections.Generic;
using RefScout.Analyzer.Config.Framework;
using RefScout.Analyzer.Notes;
using Xunit;

namespace RefScout.Analyzer.Tests;

public class AssemblyTests
{
    [Fact]
    public void Properties()
    {
        var assembly = new Assembly(AssHelp.Iden("Assembly"), "path", AssemblySource.NotFound)
        {
            IsEntryPoint = true,
            BindingRedirects = new List<BindingRedirect>()
        };
        Assert.Empty(assembly.BindingRedirects);
        Assert.Equal("path", assembly.Path);
        Assert.Equal("Assembly,1.0.0.0,neutral,null", assembly.Id);
        Assert.True(assembly.IsEntryPoint);
    }

    [Fact]
    public void ActualVersion_Unification()
    {
        var expected = Version.Parse("4.2.0.0");
        var assembly = AssHelp.Ass("UnificationAssembly", "4.0.0.0", originalVersion: expected) with
        {
            IsUnification = true
        };

        Assert.Equal(expected, assembly.ActualVersion);
        Assert.Equal("UnificationAssembly,4.2.0.0,neutral,null", assembly.Id);
    }

    [Fact]
    public void ActualVersion_NoUnification()
    {
        var assembly = AssHelp.Ass("UnificationAssembly", "4.0.0.0", originalVersion: Version.Parse("4.2.0.0"));
        Assert.Equal(Version.Parse("4.0.0.0"), assembly.ActualVersion);
    }

    [Fact]
    public void Level()
    {
        var assembly = AssHelp.Ass();
        Assert.Equal(NoteLevel.Default, assembly.Level);

        assembly.AddNote(NoteType.BindingRedirectSuccess, "success");
        Assert.Equal(NoteLevel.Success, assembly.Level);

        assembly.AddNote(NoteType.LoadNotFound, "not found");
        Assert.Equal(NoteLevel.Fatal, assembly.Level);

        Assert.NotEmpty(assembly.Notes);
        Assert.True(assembly.HasNotes);
    }
}