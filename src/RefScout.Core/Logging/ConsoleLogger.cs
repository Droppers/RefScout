using System;
using RefScout.Core.Helpers;

namespace RefScout.Core.Logging;

public class ConsoleLogger : ILogger
{
    public void LogDebug(LogEntry entry)
    {
        ConsoleHelper.WriteLine(entry.FormattedMessage, ConsoleColor.Cyan);
    }

    public void LogInfo(LogEntry entry)
    {
        Console.WriteLine(entry.FormattedMessage);
    }

    public void LogWarn(LogEntry entry)
    {
        ConsoleHelper.WriteLine(entry.FormattedMessage, ConsoleColor.Yellow);
    }

    public void LogError(LogEntry entry)
    {
        ConsoleHelper.WriteLine(entry.FormattedMessage, ConsoleColor.Red);
    }
}