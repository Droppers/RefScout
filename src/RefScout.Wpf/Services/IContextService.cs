using RefScout.Analyzer;

namespace RefScout.Wpf.Services;

internal interface IContextService
{
    Assembly? ActiveAssembly { get; set; }

    void ShowDetailsWindow(Assembly assembly);
}