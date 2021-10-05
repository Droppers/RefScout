using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RefScout.Wpf.Views.Controls;

public partial class IconButton : UserControl
{
    public static readonly RoutedEvent ClickEvent =
        EventManager.RegisterRoutedEvent("Click",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(IconButton));

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command",
        typeof(ICommand),
        typeof(IconButton),
        new PropertyMetadata(null));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register("Icon",
            typeof(string),
            typeof(IconButton),
            new FrameworkPropertyMetadata(string.Empty));

    public static readonly DependencyProperty IconWidthProperty =
        DependencyProperty.Register("IconWidth",
            typeof(int),
            typeof(IconButton),
            new FrameworkPropertyMetadata(24));

    public static readonly DependencyProperty IconHeightProperty =
        DependencyProperty.Register("IconHeight",
            typeof(int),
            typeof(IconButton),
            new FrameworkPropertyMetadata(24));

    public IconButton()
    {
        InitializeComponent();
    }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public int IconWidth
    {
        get => (int)GetValue(IconWidthProperty);
        set => SetValue(IconWidthProperty, value);
    }

    public int IconHeight
    {
        get => (int)GetValue(IconHeightProperty);
        set => SetValue(IconHeightProperty, value);
    }

    public event RoutedEventHandler Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(ClickEvent));
    }
}