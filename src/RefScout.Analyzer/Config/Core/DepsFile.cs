using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RefScout.Analyzer.Config.Core;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
[SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
public class DepsFile
{
    public Dictionary<string, Dictionary<string, RuntimeInfo>>? Targets { get; set; }
    public Dictionary<string, Library>? Libraries { get; set; }

    public class RuntimeInfo
    {
        public Dictionary<string, object>? Runtime { get; set; }
    }

    public class Library
    {
        public string? Type { get; set; }
        public string? Path { get; set; }
    }
}