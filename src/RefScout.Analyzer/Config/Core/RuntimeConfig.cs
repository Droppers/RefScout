using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace RefScout.Analyzer.Config.Core;

public class RuntimeConfig
{
    public RuntimeOptions? RuntimeOptions { get; [ExcludeFromCodeCoverage] set; }
}

public class RuntimeOptions
{
    public RollForwardBehavior RollForward { get; set; }
    public RuntimeFramework? Framework { get; set; }

    public IReadOnlyList<object> IncludedFrameworks { get; set; } = Array.Empty<object>();
}

public class RuntimeFramework
{
    public string? Name { get; set; }
    public Version? Version { get; set; }
}

public enum RollForwardBehavior
{
    Minor,
    Major,
    LatestPatch,
    LatestMinor,
    LatestMajor,
    Disable
}