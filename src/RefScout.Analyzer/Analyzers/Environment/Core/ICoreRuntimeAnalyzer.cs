using System;
using System.Collections.Generic;
using RefScout.Analyzer.Config.Core;

namespace RefScout.Analyzer.Analyzers.Environment.Core;

internal interface ICoreRuntimeAnalyzer : IRuntimeAnalyzer<CoreRuntimeAnalyzerResult>
{
    CoreRuntime? FindRuntime(
        Version requestedVersion,
        RollForwardBehavior behavior = RollForwardBehavior.Minor,
        bool? is64Bit = null);

    CoreRuntime? FindRuntime(
        Version requestedVersion,
        IEnumerable<CoreRuntime> availableRuntimes,
        RollForwardBehavior behavior,
        bool? is64Bit = null);

    IReadOnlyList<CoreRuntime> LocateAll(bool? is64Bit = null);
}