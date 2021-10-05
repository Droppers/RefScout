namespace RefScout.Core.Logging;

public interface ILogger
{
    void Log(LogEntry entry) { }

    void LogDebug(LogEntry entry) { }

    void LogInfo(LogEntry entry) { }

    void LogWarn(LogEntry entry) { }

    void LogError(LogEntry entry) { }
}