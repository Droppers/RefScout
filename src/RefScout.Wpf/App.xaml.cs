using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RefScout.Core.Logging;
using RefScout.Wpf.Services;
using RefScout.Wpf.ViewModels;
using RefScout.Wpf.Views;

namespace RefScout.Wpf;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        _serviceProvider = ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    public static IServiceProvider? ServiceProvider { get; private set; }

    private ResourceDictionary ThemeDictionary => Resources.MergedDictionaries[0];

    public void UpdateTheme()
    {
        var settings = _serviceProvider.GetRequiredService<ISettingsService>();
        var themeUri = new Uri(settings.Settings.DarkTheme ? "themes/Dark.xaml" : "themes/Light.xaml",
            UriKind.Relative);
        ThemeDictionary.MergedDictionaries.Clear();
        ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary { Source = themeUri });
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<IContextService, ContextService>();
        services.AddSingleton<ISettingsService, SettingsService>();

        services.AddTransient<AssemblyListTabViewModel>();
        services.AddTransient<EnvironmentTabViewModel>();
        services.AddTransient<TechnologiesTabViewModel>();
        services.AddTransient<GraphContainerViewModel>();
        services.AddTransient<SettingsWindowViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<DetailsWindowViewModel>();
        services.AddTransient<LoggingWindowViewModel>();

        services.AddTransient<MainWindow>();
        services.AddTransient<LoggingWindow>();
        services.AddTransient<SettingsWindow>();
        services.AddTransient<DetailsWindow>();
    }

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        Logger.Level = LogLevel.Info;

        _serviceProvider.GetRequiredService<ILoggingService>();
        var settings = _serviceProvider.GetRequiredService<ISettingsService>();
        await settings.LoadAsync();

        UpdateTheme();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private async void OnExit(object sender, ExitEventArgs e)
    {
        var settings = _serviceProvider.GetRequiredService<ISettingsService>();
        await settings.SaveAsync();
    }
}