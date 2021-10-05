using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using RefScout.Core.Logging;
using RefScout.Wpf.Views;

namespace RefScout.Wpf.Services;

internal class LoggingService : ILoggingService, ILogger
{
    private readonly IServiceProvider _serviceProvider;
    private readonly object _lock = new();

    public LoggingService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        BindingOperations.EnableCollectionSynchronization(Entries, _lock);
        Logger.AddLogger(this);
    }

    public void Log(LogEntry entry)
    {
        Entries.Add(entry);
    }

    public ObservableCollection<LogEntry> Entries { get; } = new();

    public async Task SaveAsync()
    {
        var saveFileDialog = new SaveFileDialog
        {
            Title = "Export logs to file",
            Filter = "Log file|*.log"
        };

        if (saveFileDialog.ShowDialog() == false)
        {
            return;
        }

        var sb = new StringBuilder();
        foreach (var entry in new List<LogEntry>(Entries))
        {
            sb.AppendLine(entry.FormattedMessage);
            if (entry.Exception != null)
            {
                sb.AppendLine(entry.Exception.ToString());
            }
        }

        await File.WriteAllTextAsync(saveFileDialog.FileName, sb.ToString());
    }

    public void OpenLoggingWindow()
    {
        var window = _serviceProvider.GetRequiredService<LoggingWindow>();
        window.Show();
    }
}