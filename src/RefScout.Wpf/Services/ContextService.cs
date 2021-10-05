using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using RefScout.Analyzer;
using RefScout.Wpf.Views;

namespace RefScout.Wpf.Services;

internal class ContextService : IContextService, INotifyPropertyChanged
{
    private readonly IServiceProvider _serviceProvider;
    private Assembly? _activeAssembly;

    public ContextService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Assembly? ActiveAssembly
    {
        get => _activeAssembly;
        set
        {
            _activeAssembly = value;
            OnPropertyChanged();
        }
    }

    public void ShowDetailsWindow(Assembly assembly)
    {
        ActiveAssembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        var window = _serviceProvider.GetRequiredService<DetailsWindow>();
        window.Show();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}