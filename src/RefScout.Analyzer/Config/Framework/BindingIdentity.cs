namespace RefScout.Analyzer.Config.Framework;

public record BindingIdentity
    (string Name, string Culture, PublicKeyToken Token, bool IsMachineConfig = false) : AssemblyIdentity(Name,
        Culture,
        Token);