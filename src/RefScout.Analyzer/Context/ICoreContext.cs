using RefScout.Analyzer.Analyzers.Environment.Core;
using RefScout.Analyzer.Config.Core;

namespace RefScout.Analyzer.Context;

internal interface ICoreContext : IContext<CoreConfig>
{
    CoreRuntime? Runtime { get; }
}