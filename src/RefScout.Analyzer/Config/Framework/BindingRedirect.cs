using System;

namespace RefScout.Analyzer.Config.Framework;

public record BindingRedirect(
    BindingIdentity Identity,
    Version NewVersion,
    Version MinimalOldVersion,
    Version MaximalOldVersion);