using System.Collections.Generic;

namespace RefScout.Analyzer.Config;

public interface IConfigErrorReport
{
    bool HasErrors => Errors.Count > 0;
    IReadOnlyList<ConfigError> Errors { get; }
}