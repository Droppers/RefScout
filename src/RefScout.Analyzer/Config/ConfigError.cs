namespace RefScout.Analyzer.Config;

public record ConfigError(string Message, int LineNumber = -1);