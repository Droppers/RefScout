using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using RefScout.Analyzer;
using RefScout.Analyzer.Filter;
using RefScout.Wpf.Services;

namespace RefScout.Wpf.ViewModels;

internal class AssemblyListTabViewModel : ObservableObject
{
    private readonly IContextService _context;
    private IReadOnlyList<Assembly> _assemblies;

    private IAnalyzerResult? _analyzerResult;
    private string _searchFilter = string.Empty;
    private bool _showReferencedBy;

    public AssemblyListTabViewModel(IContextService context, ISettingsService settings)
    {
        _context = context;
        _assemblies = Array.Empty<Assembly>();
        ViewDetails = new RelayCommand<Assembly>(DoViewDetails);
        SearchAssemblies = new RelayCommand<string>(DoSearchAssemblies);

        _showReferencedBy = settings.Settings.ShowReferencedBy;
        settings.Settings.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(settings.Settings.ShowReferencedBy))
            {
                ShowReferencedBy = settings.Settings.ShowReferencedBy;
            }
        };
    }

    public RelayCommand<Assembly> ViewDetails { get; }
    public RelayCommand<string> SearchAssemblies { get; }

    public IAnalyzerResult? AnalyzerResult
    {
        get => _analyzerResult;
        set => SetProperty(ref _analyzerResult, value);
    }

    public IReadOnlyList<Assembly> Assemblies
    {
        get => _assemblies;
        set => SetProperty(ref _assemblies, value);
    }

    public string SearchFilter
    {
        get => _searchFilter;
        set => SetProperty(ref _searchFilter, value);
    }

    public bool ShowReferencedBy
    {
        get => _showReferencedBy;
        set => SetProperty(ref _showReferencedBy, value);
    }


    public void OnNewAnalyzerResult(IAnalyzerResult? analyzerResult)
    {
        AnalyzerResult = analyzerResult;
        SearchFilter = string.Empty;
        Assemblies = AnalyzerResult?.Assemblies ?? Array.Empty<Assembly>();
    }

    private void DoViewDetails(Assembly? assembly)
    {
        if (assembly != null)
        {
            _context.ShowDetailsWindow(assembly);
        }
    }

    private void DoSearchAssemblies(string? filter)
    {
        if (AnalyzerResult == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(filter))
        {
            Assemblies = AnalyzerResult.Assemblies;
            return;
        }

        Assemblies = AnalyzerResult.Assemblies
            .Where(FilterParser.Parse(filter)).ToList();
    }
}