using System.Collections.Generic;
using RefScout.Analyzer.Analyzers.Environment.Framework;

namespace RefScout.Analyzer.Context;

internal interface IFrameworkContext : ISharedFrameworkContext
{
    IReadOnlyList<TargetFramework> SupportedRuntimes { get; }
    FrameworkRuntime? Runtime { get; }
}