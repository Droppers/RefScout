using System.Collections.Generic;
using System.Linq;
using RefScout.Analyzer.Helpers;
using Xunit;

namespace RefScout.Analyzer.Tests.Helpers;

public class AssemblyHelperTests
{
    [Fact]
    public void Clone()
    {
        var assemblies = ConstructAssemblies().ToList();
        var clonedAssemblies = AssemblyHelper.Clone(assemblies);

        Assert.Equal(assemblies.Count, clonedAssemblies.Count);
        Assert.Equal(assemblies.First().FullName, clonedAssemblies.First().FullName);
        Assert.Equal(assemblies.First().References.Count, clonedAssemblies.First().References.Count);
        Assert.Equal(assemblies.First().References.First().To.FullName,
            clonedAssemblies.First().References.First().To.FullName);
        Assert.Equal(assemblies.Last().ReferencedBy.Count, clonedAssemblies.Last().ReferencedBy.Count);
    }

    [Fact]
    public void Clone_WithFilter()
    {
        const string eliminatedAssembly = "ReferenceB";

        var assemblies = ConstructAssemblies().ToList();
        var clonedAssemblies = AssemblyHelper.Clone(assemblies, a => a.Name != eliminatedAssembly);

        Assert.Equal(assemblies.Count - 1, clonedAssemblies.Count);
        Assert.DoesNotContain(clonedAssemblies, a => a.Name == eliminatedAssembly);
        Assert.Contains(clonedAssemblies, a => a.Name == "EntryPoint");
        Assert.DoesNotContain(clonedAssemblies.SingleOrDefault(a => a.Name == "EntryPoint")!.References,
            a => a.To.Name == eliminatedAssembly);
    }

    private static IEnumerable<Assembly> ConstructAssemblies()
    {
        var referenceA = AssHelp.Ass("ReferenceA", "5.0.0.0");
        var referenceB = AssHelp.Ass("ReferenceB");
        var entryPoint = AssHelp.Ass("EntryPoint");
        var referenceC = AssHelp.Ass("ReferenceC", "12.0.0.32");

        var a = AssHelp.Ref(entryPoint, referenceA, "3.5.0.0");
        var b = AssHelp.Ref(entryPoint, referenceB, "2.5.0.0");
        var c = AssHelp.Ref(entryPoint, referenceC, "5.0.0.0");
        var d = AssHelp.Ref(referenceB, referenceA, "1.5.0.0");
        entryPoint.References.Add(a);
        entryPoint.References.Add(b);
        entryPoint.References.Add(c);
        referenceB.References.Add(d);

        referenceA.ReferencedBy.Add(a);
        referenceB.ReferencedBy.Add(b);
        referenceC.ReferencedBy.Add(c);
        referenceA.ReferencedBy.Add(d);

        yield return entryPoint;
        yield return referenceA;
        yield return referenceB;
        yield return referenceC;
    }
}