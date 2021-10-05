using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Notes;

internal interface INoteGenerator
{
    void Generate(IContext context);
}