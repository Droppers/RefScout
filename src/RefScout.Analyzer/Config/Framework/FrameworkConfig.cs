using System;
using System.Collections.Generic;

namespace RefScout.Analyzer.Config.Framework;

public record FrameworkConfig : IConfig
{
    public IReadOnlyList<TargetFramework> SupportedRuntimes { get; init; } = Array.Empty<TargetFramework>();
    public IReadOnlyList<string> ProbeFolders { get; init; } = Array.Empty<string>();
    public IReadOnlyList<BindingRedirect> BindingRedirects { get; init; } = Array.Empty<BindingRedirect>();
    public IReadOnlyList<CodeBase> CodeBases { get; init; } = Array.Empty<CodeBase>();
    public IConfigErrorReport ErrorReport { get; init; } = new FrameworkConfigErrorReport();
}