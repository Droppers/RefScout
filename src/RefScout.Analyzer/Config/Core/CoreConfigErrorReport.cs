using System;
using System.Collections.Generic;

namespace RefScout.Analyzer.Config.Core;

internal class CoreConfigErrorReport : IConfigErrorReport
{
    public IReadOnlyList<ConfigError> Errors { get; } = Array.Empty<ConfigError>();

    public override string ToString() => "Core config validation not implemented.";
}