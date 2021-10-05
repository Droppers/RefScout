namespace RefScout.Analyzer.Config.Framework;

public record FrameworkConfigError(string Element, string Message, int LineNumber = -1) : ConfigError(Message,
    LineNumber);