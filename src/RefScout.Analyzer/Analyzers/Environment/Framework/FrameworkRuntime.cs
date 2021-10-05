using System;
using System.Diagnostics.CodeAnalysis;

namespace RefScout.Analyzer.Analyzers.Environment.Framework;

[ExcludeFromCodeCoverage]
public record FrameworkRuntime(Version RuntimeVersion, Version Version, int ServicePack = -1);