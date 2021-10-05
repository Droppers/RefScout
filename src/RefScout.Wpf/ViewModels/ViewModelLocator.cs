using Microsoft.Extensions.DependencyInjection;

namespace RefScout.Wpf.ViewModels;

internal class ViewModelLocator
{
    public static MainWindowViewModel MainWindowViewModel =>
        App.ServiceProvider!.GetRequiredService<MainWindowViewModel>();

    public static SettingsWindowViewModel SettingsWindowViewModel =>
        App.ServiceProvider!.GetRequiredService<SettingsWindowViewModel>();

    public static DetailsWindowViewModel DetailsWindowViewModel =>
        App.ServiceProvider!.GetRequiredService<DetailsWindowViewModel>();

    public static LoggingWindowViewModel LoggingWindowViewModel =>
        App.ServiceProvider!.GetRequiredService<LoggingWindowViewModel>();

    public static AssemblyListTabViewModel AssemblyListTabViewModel =>
        App.ServiceProvider!.GetRequiredService<AssemblyListTabViewModel>();

    public static TechnologiesTabViewModel TechnologiesTabViewModel =>
        App.ServiceProvider!.GetRequiredService<TechnologiesTabViewModel>();

    public static GraphContainerViewModel GraphContainerViewModel =>
        App.ServiceProvider!.GetRequiredService<GraphContainerViewModel>();

    public static EnvironmentTabViewModel EnvironmentTabViewModel =>
        App.ServiceProvider!.GetRequiredService<EnvironmentTabViewModel>();
}