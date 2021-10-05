using System;
using System.Collections.Generic;
using System.Linq;
using RefScout.Analyzer.Config.Framework;
using RefScout.Analyzer.Helpers;
using RefScout.Analyzer.Notes;

namespace RefScout.Analyzer;

public record Assembly : AssemblyIdentity
{
    private readonly List<ConflictNote> _notes;

    public Assembly(AssemblyIdentity identity, string? path, AssemblySource source)
        : base(identity.Name, identity.Culture, identity.PublicKeyToken, identity.Version)
    {
        _notes = new List<ConflictNote>();
        RawReferences = Array.Empty<AssemblyIdentity>();
        References = new List<AssemblyRef>();
        ReferencedBy = new List<AssemblyRef>();
        BindingRedirects = Array.Empty<BindingRedirect>();

        OriginalVersion = identity.Version;
        Path = path;
        Source = source;

        IsSystem = MicrosoftHelper.IsSystemAssembly(Name);
        IsNetApi = MicrosoftHelper.IsNetApi(Name);
    }

    public Assembly(AssemblyIdentity identity, AssemblySource source)
        : this(identity, null, source) { }

    public override string Id =>
        $"{Name},{(IsUnification && OriginalVersion != null ? OriginalVersion : Version)},{Culture},{PublicKeyToken}";

    public override string FullName =>
        $"{Name}, Version={(IsUnification && OriginalVersion != null ? OriginalVersion : Version)}, Culture={Culture}, PublicKeyToken={PublicKeyToken}";

    public bool IsEntryPoint { get; init; }
    public bool IsArchitectureMismatch { get; internal set; }

    public IReadOnlyList<AssemblyIdentity> RawReferences { get; internal set; }
    public string? ProcessorArchitectureString { get; internal set; }
    public ProcessorArchitecture ProcessorArchitecture { get; internal set; }
    public bool Is64Bit { get; internal set; }
    public TargetFramework? TargetFramework { get; internal set; }
    public AssemblySourceLanguage SourceLanguage { get; internal set; } = AssemblySourceLanguage.Unknown;
    public AssemblyKind Kind { get; internal set; } = AssemblyKind.Dll;

    public bool IsSystem { get; init; }
    public bool IsLocalSystem { get; internal set; }
    public bool IsNetApi { get; init; }

    public Version? OriginalVersion { get; init; }
    public Version ActualVersion => IsUnification && OriginalVersion != null ? OriginalVersion : Version;
    public bool IsUnification { get; init; }

    public string? Path { get; }
    public CodeBase? CodeBase { get; init; }
    public IReadOnlyList<BindingRedirect> BindingRedirects { get; internal set; }

    public bool IsUnreferenced { get; init; }
    public bool HasNotes => _notes.Count > 0;
    public IReadOnlyList<ConflictNote> Notes => _notes;

    public NoteLevel Level =>
        _notes.Count > 0 ? (NoteLevel)_notes.Max(a => (int)a.Type) : NoteLevel.Default;

    public List<AssemblyRef> References { get; internal set; }
    public List<AssemblyRef> ReferencedBy { get; internal set; }

    public AssemblySource Source { get; init; }

    public void AddNote(NoteType type, string message)
    {
        _ = message ?? throw new ArgumentNullException(nameof(message));
        _notes.Add(new ConflictNote(type, message));
    }
}

public enum AssemblySource
{
    Local,
    NotFound,
    Error,
    Gac,
    Shared,
    CodeBase
}

public enum AssemblySourceLanguage
{
    CSharp = 1,
    VbNet = 2,
    FSharp = 3,
    CppCli = 4,
    Unknown = 5
}

public enum ProcessorArchitecture
{
    Unknown,
    Cil,
    Amd64,
    Arm,
    Arm64,
    Ia64,
    X86
}

public enum AssemblyKind
{
    Dll,
    Web,
    Console,
    Windows
}