using System;

namespace RefScout.Analyzer.Config.Framework;

public record CodeBase(BindingIdentity Identity, Version Version, string RelativeHref, string AbsoluteHref);