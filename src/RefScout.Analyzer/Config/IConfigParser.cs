namespace RefScout.Analyzer.Config;

internal interface IConfigParser<out TConfig> where TConfig : IConfig
{
    TConfig ParseFile(string assemblyFileName, string? configFileName);
}