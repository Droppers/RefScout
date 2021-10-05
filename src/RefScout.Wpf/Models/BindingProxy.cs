using System.Windows;

namespace RefScout.Wpf.Models;

public class BindingProxy : Freezable
{
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    protected override Freezable CreateInstanceCore() => new BindingProxy();
}