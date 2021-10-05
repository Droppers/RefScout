using System.Collections.Generic;
using System.Threading;
using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Analyzers.Assemblies;

internal class ReferencedAssembliesAnalyzer : IAssemblyAnalyzer
{
    public void Analyze(IContext context, AnalyzerOptions options, CancellationToken cancellationToken)
    {
        var assembliesToVisit = new Queue<Assembly>();
        assembliesToVisit.Enqueue(context.EntryPoint);
        while (assembliesToVisit.Count > 0)
        {
            var assembly = assembliesToVisit.Dequeue();
            foreach (var reference in assembly.RawReferences)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var existingAssembly = context.Find(reference);
                var toAssembly = existingAssembly ?? context.Resolve(reference);

                if (ShouldNotAnalyzeReference(options.AnalyzeMode, toAssembly))
                {
                    continue;
                }

                // Create a reference to the assembly
                var assemblyRef = new AssemblyRef(assembly, toAssembly, reference.Version);
                assembly.References.Add(assemblyRef);
                toAssembly.ReferencedBy.Add(assemblyRef);

                if (ShouldEnqueue(context, options.AnalyzeMode, toAssembly))
                {
                    assembliesToVisit.Enqueue(toAssembly);
                }

                if (existingAssembly == null)
                {
                    context.Add(toAssembly);
                }
            }
        }
    }

    // Local system assemblies are always checked for .NET framework, the assumption is made that
    // when an application ships with system assemblies this is done for a reason (ILSpy for example).
    private static bool ShouldNotAnalyzeReference(AnalyzeMode analyzeMode, Assembly assembly) =>
        analyzeMode is not (AnalyzeMode.AppDirectSystem or AnalyzeMode.All) &&
        assembly.IsSystem && !assembly.IsLocalSystem;

    private static bool ShouldEnqueue(IContext context, AnalyzeMode analyzeMode, Assembly assembly)
    {
        var enqueueSystemAssembly = analyzeMode == AnalyzeMode.All ||
                                    !assembly.IsSystem || assembly.IsLocalSystem;
        return assembly.RawReferences.Count > 0 && !context.Contains(assembly) && enqueueSystemAssembly;
    }
}