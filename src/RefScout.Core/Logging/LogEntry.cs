using System;

namespace RefScout.Core.Logging;

public record LogEntry(LogLevel Level, string Message, string FormattedMessage, Exception? Exception = null);