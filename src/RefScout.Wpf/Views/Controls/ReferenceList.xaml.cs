using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using RefScout.Analyzer;

namespace RefScout.Wpf.Views.Controls;

public partial class ReferenceList : UserControl
{
    public static readonly DependencyProperty ReferencesProperty =
        DependencyProperty.Register("References",
            typeof(IReadOnlyList<AssemblyRef>),
            typeof(ReferenceList),
            new PropertyMetadata(Array.Empty<AssemblyRef>()));

    public static readonly DependencyProperty ReferencedByProperty =
        DependencyProperty.Register("ReferencedBy",
            typeof(bool),
            typeof(ReferenceList),
            new PropertyMetadata(false));

    public ReferenceList()
    {
        InitializeComponent();
    }

    public IReadOnlyList<AssemblyRef> References
    {
        get => (IReadOnlyList<AssemblyRef>)GetValue(ReferencesProperty);
        set => SetValue(ReferencesProperty, value);
    }

    public bool ReferencedBy
    {
        get => (bool)GetValue(ReferencedByProperty);
        set => SetValue(ReferencedByProperty, value);
    }
}