using System;
using System.Collections.Generic;
using System.Linq;
using RefScout.Analyzer.Analyzers.Compatibility;
using RefScout.Analyzer.Config.Framework;
using RefScout.Analyzer.Notes;

namespace RefScout.Analyzer;

public record AssemblyRef(Assembly From, Assembly To, Version Version)
{
    private readonly List<ConflictNote> _notes = new();

    // ActualVersion means version with binding redirect applied
    public Version ActualVersion => BindingRedirect?.NewVersion ?? Version;
    public BindingRedirect? BindingRedirect { get; set; }
    public CodeBase? CodeBase { get; set; }

    public ReferenceCompatibility Compatibility { get; internal set; }
    public BindingRedirectStatus BindingRedirectStatus { get; internal set; }

    public bool HasNotes => _notes.Count > 0;
    public IReadOnlyList<ConflictNote> Notes => _notes;

    public NoteLevel Level =>
        _notes.Count > 0 ? (NoteLevel)_notes.Max(a => (int)a.Type) : NoteLevel.Default;

    public void AddNote(NoteType type, string message)
    {
        _ = message ?? throw new ArgumentNullException(nameof(message));
        _notes.Add(new ConflictNote(type, message));
    }
}

public enum BindingRedirectStatus
{
    Default,
    Failed,
    FailedWrongVersion,
    Success
}