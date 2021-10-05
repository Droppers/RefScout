using System.Collections.Generic;
using System.Linq;
using RefScout.Analyzer;

namespace RefScout.Wpf.Models;

internal class LanguageGroup
{
    public LanguageGroup(AssemblySourceLanguage language, IEnumerable<Assembly> assemblies)
    {
        Language = language;
        Assemblies = assemblies.ToList();
    }

    public AssemblySourceLanguage Language { get; set; }
    public IReadOnlyList<Assembly> Assemblies { get; set; }
}