using System;
using System.Collections.Generic;
using System.Linq;

namespace RefScout.Analyzer.Helpers;

public static class AssemblyHelper
{
    public static List<Assembly> Clone(
        IEnumerable<Assembly> assemblies,
        Func<Assembly, bool>? predicate = null)
    {
        var result = predicate != null
            ? assemblies.Where(predicate).Select(a => a with { }).ToList()
            : assemblies.Select(a => a with { }).ToList();
        foreach (var assembly in result)
        {
            assembly.References = CloneAssemblyReferences(result, assembly.References);
            assembly.ReferencedBy = CloneAssemblyReferences(result, assembly.ReferencedBy);
        }

        return result;
    }

    private static List<AssemblyRef> CloneAssemblyReferences(
        IReadOnlyList<Assembly> assemblies,
        IEnumerable<AssemblyRef> references)
    {
        return references.Where(x =>
                assemblies.Any(a => a.FullName == x.From.FullName) &&
                assemblies.Any(a => a.FullName == x.To.FullName))
            .Select(r => r with
            {
                From = assemblies.Single(x => x.FullName == r.From.FullName),
                To = assemblies.Single(x => x.FullName == r.To.FullName)
            }).ToList();
    }
}