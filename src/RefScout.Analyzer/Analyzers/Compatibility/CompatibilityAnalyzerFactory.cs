using System;
using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Analyzers.Compatibility;

internal class CompatibilityAnalyzerFactory : ICompatibilityAnalyzerFactory
{
    public CompatibilityAnalyzer Create(IContext context, IVersionComparer comparer)
    {
        return context switch
        {
            ICoreContext => new CoreCompatibilityAnalyzer(context, comparer),
            ISharedFrameworkContext frameworkContext => new SharedFrameworkCompatibilityAnalyzer(frameworkContext,
                comparer),
            _ => throw new NotSupportedException(
                $"Context type {context.GetType().Name} not supported for {GetType().Name}")
        };
    }
}