using System;
using System.Collections.Generic;
using RefScout.Analyzer.Config.Framework;

namespace RefScout.Analyzer.Context;

internal interface ISharedFrameworkContext : IContext<FrameworkConfig>
{
    FrameworkConfig? MachineConfig { get; }

    BindingRedirect? FindBindingRedirect(string name, Version version);
    IReadOnlyList<BindingRedirect> FindBindingRedirects(AssemblyIdentity identity);
    CodeBase? FindCodeBase(AssemblyIdentity identity, Version version);
}