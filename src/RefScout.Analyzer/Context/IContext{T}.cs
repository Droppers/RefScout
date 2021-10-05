using RefScout.Analyzer.Config;

namespace RefScout.Analyzer.Context;

internal interface IContext<out TConfig> : IContext where TConfig : IConfig
{
    new TConfig Config { get; }
    IConfig IContext.Config => Config;
}