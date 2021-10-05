using System.Collections.Generic;
using System.Linq;
using RefScout.Analyzer;

namespace RefScout.Wpf.Models;

internal class FrameworkGroup
{
    public FrameworkGroup(TargetFramework targetFramework, IEnumerable<Assembly> assemblies)
    {
        TargetFramework = targetFramework;
        Assemblies = assemblies.ToList();
    }

    public TargetFramework TargetFramework { get; set; }
    public IReadOnlyList<Assembly> Assemblies { get; set; }
}