using System.Linq;
using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Analyzers.Compatibility;

internal class CompatibilityAnalyzer : ICompatibilityAnalyzer
{
    private static readonly string[] IgnoreArchitectureMismatch =
    {
        "mscorlib",
        "PresentationCore",
        "System.Data",
        "System.Data.OracleClient",
        "System.EnterpriseServices",
        "System.Printing",
        "System.Transactions",
        "System.Web"
    };

    private readonly IContext _context;
    protected readonly IVersionComparer Comparer;

    protected CompatibilityAnalyzer(IContext context, IVersionComparer comparer)
    {
        _context = context;
        Comparer = comparer;
    }

    public void Analyze()
    {
        foreach (var assembly in _context.Assemblies)
        {
            foreach (var reference in assembly.References)
            {
                CheckReference(reference);
            }

            CheckAssembly(assembly);
        }
    }

    protected virtual void CheckAssembly(Assembly assembly)
    {
        if (assembly.Source is not AssemblySource.Shared)
        {
            assembly.IsArchitectureMismatch = IsArchitectureMismatch(assembly, _context.EntryPoint);
        }
    }

    protected virtual void CheckReference(AssemblyRef reference)
    {
        reference.Compatibility = Comparer.Compare(reference.ActualVersion, reference.To, false);
    }

    private static bool IsArchitectureMismatch(Assembly assembly, Assembly entryAssembly) =>
        assembly.ProcessorArchitecture != ProcessorArchitecture.Unknown &&
        assembly.ProcessorArchitecture != ProcessorArchitecture.Cil &&
        assembly.ProcessorArchitecture != entryAssembly.ProcessorArchitecture &&
        !IgnoreArchitectureMismatch.Contains(assembly.Name) && !assembly.IsSystem;
}