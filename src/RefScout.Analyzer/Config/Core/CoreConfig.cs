using System;
using RefScout.Analyzer.Analyzers.Environment.Core;

namespace RefScout.Analyzer.Config.Core;

public class CoreConfig : IConfig
{
    public CoreConfig(RuntimeConfig runtimeConfig, DepsFile depsFile)
    {
        RuntimeConfig = runtimeConfig;
        DepsFile = depsFile;
    }

    public RuntimePack RuntimePack { get; init; }
    public RollForwardBehavior RollForward { get; init; }
    public bool SelfContained { get; init; }
    public Version? TargetRuntimeVersion { get; set; }

    public RuntimeConfig RuntimeConfig { get; }
    public DepsFile DepsFile { get; }

    public IConfigErrorReport ErrorReport { get; } = new CoreConfigErrorReport();
}