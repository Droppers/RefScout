using RefScout.Analyzer.Analyzers.Environment.Mono;

namespace RefScout.Analyzer.Context;

internal interface IMonoContext : ISharedFrameworkContext
{
    MonoRuntime? Runtime { get; }
}