﻿using System.Windows;
using System.Windows.Controls;
using RefScout.Analyzer;
using RefScout.Wpf.ViewModels;

namespace RefScout.Wpf.Views;

public partial class EnvironmentTab : UserControl
{
    public static readonly DependencyProperty AnalyzerResultProperty =
        DependencyProperty.Register("AnalyzerResult",
            typeof(IAnalyzerResult),
            typeof(EnvironmentTab),
            new FrameworkPropertyMetadata(null, OnAnalyzerResultChanged));

    public EnvironmentTab()
    {
        InitializeComponent();
    }

    public IAnalyzerResult AnalyzerResult
    {
        get => (IAnalyzerResult)GetValue(AnalyzerResultProperty);
        set => SetValue(AnalyzerResultProperty, value);
    }

    private static void OnAnalyzerResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var container = (EnvironmentTab)d;
        var context = (EnvironmentTabViewModel)container.DataContext;
        context.OnNewAnalyzerResult(e.NewValue as IAnalyzerResult);
    }
}