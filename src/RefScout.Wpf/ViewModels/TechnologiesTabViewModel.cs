using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using RefScout.Analyzer;
using RefScout.Wpf.Models;

namespace RefScout.Wpf.ViewModels;

internal class TechnologiesTabViewModel : ObservableObject
{
    private IAnalyzerResult? _analyzerResult;
    private IReadOnlyList<FrameworkGroup> _frameworkGroups;
    private IReadOnlyList<LanguageGroup> _languageGroups;

    public TechnologiesTabViewModel()
    {
        _frameworkGroups = Array.Empty<FrameworkGroup>();
        _languageGroups = Array.Empty<LanguageGroup>();
    }

    public IAnalyzerResult? AnalyzerResult
    {
        get => _analyzerResult;
        set => SetProperty(ref _analyzerResult, value);
    }

    public IReadOnlyList<FrameworkGroup> FrameworkGroups
    {
        get => _frameworkGroups;
        private set => SetProperty(ref _frameworkGroups, value);
    }

    public IReadOnlyList<LanguageGroup> LanguageGroups
    {
        get => _languageGroups;
        private set => SetProperty(ref _languageGroups, value);
    }

    public void OnNewAnalyzerResult(IAnalyzerResult? analyzerResult)
    {
        AnalyzerResult = analyzerResult;

        if (analyzerResult == null)
        {
            return;
        }

        static bool FilterPredicate(Assembly a) =>
            a.Source is not (AssemblySource.NotFound or AssemblySource.Error) &&
            !a.IsUnreferenced && !a.IsSystem && !a.IsNetApi;

        FrameworkGroups = analyzerResult.Assemblies
            .Where(FilterPredicate)
            .OrderBy(a => a.Name)
            .GroupBy(u => u.TargetFramework?.Id)
            .Select(l => new FrameworkGroup(l.First().TargetFramework!, l))
            .OrderByDescending(g => g.TargetFramework.Version)
            .ToList();

        LanguageGroups = analyzerResult.Assemblies
            .Where(FilterPredicate)
            .OrderBy(a => a.Name)
            .GroupBy(u => u.SourceLanguage)
            .Select(l => new LanguageGroup(l.First().SourceLanguage, l))
            .OrderBy(a => (int)a.Language)
            .ToList();
    }
}