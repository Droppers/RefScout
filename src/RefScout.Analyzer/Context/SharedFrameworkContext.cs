using System;
using System.Collections.Generic;
using System.Linq;
using RefScout.Analyzer.Analyzers.Compatibility;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Config.Framework;
using RefScout.Analyzer.Readers;
using RefScout.Analyzer.Resolvers;

namespace RefScout.Analyzer.Context;

internal class SharedFrameworkContext : Context, ISharedFrameworkContext
{
    protected SharedFrameworkContext(
        IResolver resolver,
        IAssemblyReader reader,
        EnvironmentInfo environmentInfo,
        FrameworkConfig? machineConfig,
        FrameworkConfig config,
        Assembly entryPoint) : base(reader, resolver, environmentInfo, entryPoint)
    {
        MachineConfig = machineConfig;
        Config = config;
    }

    public FrameworkConfig? MachineConfig { get; }

    public override FrameworkConfig Config { get; }

    public override Assembly? Find(AssemblyIdentity identity)
    {
        _ = identity ?? throw new ArgumentNullException(nameof(identity));

        var version = FindBindingRedirect(identity.Name, identity.Version)?.NewVersion ?? identity.Version;
        var codeBase = FindCodeBase(identity, version);
        if (codeBase != null)
        {
            // Only compare CodeBase assemblies to CodeBase assemblies
            return identity.IsStrongNamed
                ? Assemblies.SingleOrDefault(a =>
                    a.Name == identity.Name && a.CodeBase?.Version == codeBase.Version)
                : Assemblies.FirstOrDefault(a =>
                    a.Name == identity.Name && a.CodeBase is not null);
        }

        // First try to find an exact version, then fallback to first assembly found
        var assembly = Assemblies.SingleOrDefault(a => a.Name == identity.Name && a.ActualVersion == version);
        return assembly ?? Assemblies.FirstOrDefault(a => a.Name == identity.Name);
    }

    public override Assembly Resolve(AssemblyIdentity identity)
    {
        _ = identity ?? throw new ArgumentNullException(nameof(identity));

        // When only application assemblies are analyzed, system assemblies may still be loaded
        // but are not present in the assemblies list
        if (Cache.TryGetValue(identity.FullName, out var cachedAssembly))
        {
            return cachedAssembly;
        }

        _ = identity ?? throw new ArgumentNullException(nameof(identity));

        // Use redirected version if binding redirect exists
        var redirect = FindBindingRedirect(identity.Name, identity.Version);
        identity = identity with
        {
            Version = redirect?.NewVersion ?? identity.Version
        };

        // Resolve from CodeBase if CodeBase entry exists
        var codeBase = FindCodeBase(identity, identity.Version);
        var assembly = codeBase != null
            ? ResolveFromCodeBase(identity, codeBase)
            : ResolveFromResolver(identity);

        // Apply metadata
        assembly.BindingRedirects = FindBindingRedirects(identity);
        assembly.IsLocalSystem = IsLocalSystem(assembly);

        Cache[identity.FullName] = assembly;

        return assembly;
    }

    // Use First rather than Single because duplicate redirects can be configured (e.g. Roslyns' csi.exe.config)
    public BindingRedirect? FindBindingRedirect(string name, Version version)
    {
        bool ShouldRedirect(BindingRedirect c) => c.Identity.Name == name
                                                  && version >= c.MinimalOldVersion
                                                  && version <= c.MaximalOldVersion;

        var machineRedirect = MachineConfig?.BindingRedirects.FirstOrDefault(ShouldRedirect);
        return machineRedirect ?? Config.BindingRedirects.FirstOrDefault(ShouldRedirect);
    }

    public IReadOnlyList<BindingRedirect> FindBindingRedirects(AssemblyIdentity identity)
    {
        bool DoesApply(BindingRedirect x) => x.Identity.Name == identity.Name &&
                                             DefaultVersionComparer.AreVersionsCompatible(x.NewVersion,
                                                 identity.Version);

        var applicationRedirects = Config.BindingRedirects.Where(DoesApply).ToList();

        var machineRedirects = MachineConfig?.BindingRedirects.Where(DoesApply);
        if (machineRedirects != null)
        {
            applicationRedirects.InsertRange(0, machineRedirects);
        }

        return applicationRedirects;
    }

    public CodeBase? FindCodeBase(AssemblyIdentity identity, Version version)
    {
        // Use first codeBase found if not strong named identity (I am not sure anymore if this is correct, this is pretty old)
        // https://docs.microsoft.com/en-us/dotnet/framework/configure-apps/file-schema/runtime/codebase-element#remarks
        bool ShouldApplyCodeBase(CodeBase c) =>
            c.Identity.Name == identity.Name && (!identity.IsStrongNamed || c.Version == version);

        var machineRedirect = MachineConfig?.CodeBases.FirstOrDefault(ShouldApplyCodeBase);
        return machineRedirect ?? Config.CodeBases.FirstOrDefault(ShouldApplyCodeBase);
    }

    private Assembly ResolveFromCodeBase(AssemblyIdentity identity, CodeBase codeBase) =>
        Reader.ReadOrDefault(codeBase.AbsoluteHref, AssemblySource.CodeBase, identity) with
        {
            CodeBase = codeBase,
            OriginalVersion = identity.Version
        };

    private bool IsLocalSystem(Assembly assembly) =>
        EntryPoint.TargetFramework?.Runtime == NetRuntime.Framework
        && assembly.Source == AssemblySource.Local
        && assembly.IsSystem;
}