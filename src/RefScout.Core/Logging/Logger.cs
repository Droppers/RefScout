using System;
using System.Collections.Generic;
using System.Text;

namespace RefScout.Core.Logging;

public enum LogLevel
{
    Debug,
    Info,
    Warn,
    Error
}

public abstract class Logger
{
    private static readonly List<ILogger> Loggers = new();

    public static LogLevel Level { get; set; } = LogLevel.Warn;

    public static void AddLogger(ILogger logger)
    {
        Loggers.Add(logger);
    }

    private static void Log(LogLevel level, string message, Exception? exception = null)
    {
        if (Level > level || Loggers.Count == 0)
        {
            return;
        }

        var formattedMessage = FormatMessage(level, message, exception);
        var entry = new LogEntry(level, message, formattedMessage, exception);
        foreach (var logger in Loggers)
        {
            logger.Log(entry);
            switch (level)
            {
                case LogLevel.Debug:
                    logger.LogDebug(entry);
                    break;
                case LogLevel.Info:
                    logger.LogInfo(entry);
                    break;
                case LogLevel.Warn:
                    logger.LogWarn(entry);
                    break;
                case LogLevel.Error:
                    logger.LogError(entry);
                    break;
            }
        }
    }

    public static void Info(string message) => Log(LogLevel.Info, message);

    public static void Warn(string message) => Log(LogLevel.Warn, message);

    public static void Warn(Exception exception, string message) => Log(LogLevel.Warn, message, exception);

    public static void Error(string message) => Log(LogLevel.Error, message);

    public static void Error(Exception exception, string message) => Log(LogLevel.Error, message, exception);

    private static string FormatMessage(LogLevel level, string message, Exception? exception)
    {
        var sb = new StringBuilder();
        sb.Append('[');
        sb.Append(level.ToString().ToUpper().PadRight(5, ' '));
        sb.Append("] ");
        sb.Append(message);

        if (exception != null)
        {
            sb.Append(": ").Append(exception.Message);
        }

        return sb.ToString();
    }
}