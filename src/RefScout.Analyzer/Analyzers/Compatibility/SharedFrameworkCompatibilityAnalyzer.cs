using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Analyzers.Compatibility;

internal class SharedFrameworkCompatibilityAnalyzer : CompatibilityAnalyzer
{
    private readonly ISharedFrameworkContext _context;

    public SharedFrameworkCompatibilityAnalyzer(ISharedFrameworkContext context, IVersionComparer comparer) : base(
        context,
        comparer)
    {
        _context = context;
    }

    protected override void CheckReference(AssemblyRef reference)
    {
        reference.BindingRedirect = _context.FindBindingRedirect(reference.To.Name, reference.Version);
        reference.CodeBase =
            _context.FindCodeBase(reference.To, reference.BindingRedirect?.NewVersion ?? reference.Version);

        // Don't threat strong named assemblies as special for Silverlight applications, it runs in its own sandbox
        var ignoreStrongNamed = reference.From.TargetFramework?.Runtime == NetRuntime.Silverlight;
        reference.Compatibility =
            Comparer.Compare(reference.ActualVersion, reference.To, ignoreStrongNamed);

        // Always run this after the version compatibility check
        reference.BindingRedirectStatus =
            FindBindingRedirectStatus(_context, reference, reference.Compatibility);
    }

    private static BindingRedirectStatus FindBindingRedirectStatus(
        ISharedFrameworkContext context,
        AssemblyRef reference,
        ReferenceCompatibility compatibility)
    {
        var allBindingRedirects = context.FindBindingRedirects(reference.To);

        // We don't care about binding redirects when:
        //  - there are no binding redirects
        //  - version is covered by binding redirect but it is redirected to the already referenced version
        if (allBindingRedirects.Count == 0 ||
            reference.BindingRedirect?.NewVersion == reference.Version)
        {
            return BindingRedirectStatus.Default;
        }

        if (reference.BindingRedirect != null && compatibility != ReferenceCompatibility.Compatible)
        {
            // Redirect worked, but redirected to wrong version (they are not compatible)
            return BindingRedirectStatus.FailedWrongVersion;
        }

        return reference.BindingRedirect != null ? BindingRedirectStatus.Success : BindingRedirectStatus.Failed;
    }
}