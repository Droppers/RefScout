using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using RefScout.Analyzer;

namespace RefScout.Wpf.Views;

public partial class TreeTab : UserControl
{
    public static readonly DependencyProperty RootAssembliesProperty =
        DependencyProperty.Register("RootAssemblies",
            typeof(IReadOnlyList<Assembly>),
            typeof(TreeTab),
            new FrameworkPropertyMetadata(Array.Empty<Assembly>()));

    public TreeTab()
    {
        InitializeComponent();
    }

    public IReadOnlyList<Assembly> RootAssemblies
    {
        get => (IReadOnlyList<Assembly>)GetValue(RootAssembliesProperty);
        set => SetValue(RootAssembliesProperty, value);
    }

    private void OnTargetUpdated(object? sender, DataTransferEventArgs e)
    {
        var items = AssemblyTreeView.Items;
        if (items.Count <= 0)
        {
            return;
        }

        var item = items[0];
        if (AssemblyTreeView.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem treeItem)
        {
            treeItem.IsExpanded = true;
        }
    }
}