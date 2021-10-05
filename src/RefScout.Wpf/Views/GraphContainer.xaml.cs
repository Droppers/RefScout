using System;
using System.Windows;
using System.Windows.Controls;
using RefScout.Analyzer;
using RefScout.Wpf.ViewModels;

namespace RefScout.Wpf.Views;

public partial class GraphContainer : UserControl
{
    public static readonly DependencyProperty AnalyzerResultProperty =
        DependencyProperty.Register("AnalyzerResult",
            typeof(IAnalyzerResult),
            typeof(GraphContainer),
            new FrameworkPropertyMetadata(null, OnAnalyzerResultChanged));

    public GraphContainer()
    {
        InitializeComponent();
    }

    public IAnalyzerResult AnalyzerResult
    {
        get => (IAnalyzerResult)GetValue(AnalyzerResultProperty);
        set => SetValue(AnalyzerResultProperty, value);
    }

    private static async void OnAnalyzerResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var container = (GraphContainer)d;
        var context = (GraphContainerViewModel)container.DataContext;
        context.AnalyzerResult = (IAnalyzerResult)e.NewValue;
        await context.RenderAnalyzerResultAsync();
    }

    private void OnGraphRendered(object? _, EventArgs e)
    {
        var d = (GraphContainerViewModel)DataContext;
        d.LoadingMessage = null;
    }

    private void OnGraphRenderFailed(object? _, Exception e)
    {
        var d = (GraphContainerViewModel)DataContext;
        d.LoadingMessage = null;
        d.ErrorMessage = e.Message;
    }

    private void OnNodeClicked(object? _, string id)
    {
        var d = (GraphContainerViewModel)DataContext;
        d.OpenDetailsWindow(id);
    }
}