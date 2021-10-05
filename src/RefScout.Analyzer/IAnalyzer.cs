using System.Diagnostics.CodeAnalysis;
using System.Threading;
using RefScout.Analyzer.Analyzers.Compatibility;

namespace RefScout.Analyzer;

public interface IAnalyzer
{
    IAnalyzerResult Analyze(
        string fileName,
        AnalyzerOptions? options = null,
        CancellationToken? cancellationToken = null);
}

[ExcludeFromCodeCoverage]
public record AnalyzerOptions
{
    public string? Config { get; set; }
    public string? Filter { get; init; }

    public AnalyzeMode AnalyzeMode { get; init; }

    public IVersionComparer? VersionComparer { get; init; }
    public VersionCompatibilityMode SystemVersionMode { get; init; }

    public AnalyzeRuntime AnalyzeRuntime { get; init; }
}

public enum AnalyzeMode
{
    AppDirectSystem,
    App,
    All
}