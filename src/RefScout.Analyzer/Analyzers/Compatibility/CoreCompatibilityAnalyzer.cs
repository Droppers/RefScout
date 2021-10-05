using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Analyzers.Compatibility;

internal class CoreCompatibilityAnalyzer : CompatibilityAnalyzer
{
    public CoreCompatibilityAnalyzer(IContext context, IVersionComparer comparer) : base(context,
        comparer) { }


    protected override void CheckReference(AssemblyRef reference)
    {
        reference.Compatibility = Comparer.Compare(reference.ActualVersion, reference.To, true);
    }
}