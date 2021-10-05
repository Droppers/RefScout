using Microsoft.Toolkit.Mvvm.ComponentModel;
using RefScout.Analyzer;
using RefScout.Wpf.Services;

namespace RefScout.Wpf.ViewModels;

internal class DetailsWindowViewModel : ObservableObject
{
    private Assembly _assembly;
    private bool _wasFound;

    public DetailsWindowViewModel(IContextService context)
    {
        _assembly = context.ActiveAssembly ??
                    new Assembly(new AssemblyIdentity("Loading"), null, AssemblySource.NotFound);
        _wasFound = Assembly.Source is not (AssemblySource.Error or AssemblySource.NotFound);
    }

    public Assembly Assembly
    {
        get => _assembly;
        set => SetProperty(ref _assembly, value);
    }

    public bool WasFound
    {
        get => _wasFound;
        set => SetProperty(ref _wasFound, value);
    }
}