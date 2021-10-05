using System;
using System.Collections.Generic;
using RefScout.Analyzer.Config.Framework;
using RefScout.Analyzer.Context;
using RefScout.Analyzer.Readers;
using RefScout.Analyzer.Resolvers;
using Xunit;

namespace RefScout.Analyzer.Tests.Context;

public class FrameworkContextTests
{
    private readonly FrameworkConfig _config;
    private readonly IResolver _resolver;
    private readonly IAssemblyReader _reader;

    public FrameworkContextTests()
    {
        _config = new FrameworkConfig
        {
            CodeBases = new[]
            {
                new CodeBase(AssHelp.BindIden("ReferenceA"), new Version("5.0.0.0"), "ReferenceACodeBase.dll",
                    "ReferenceACodeBase.dll")
            },
            BindingRedirects = new[]
            {
                new BindingRedirect(AssHelp.BindIden("ReferenceA"), new Version("5.0.0.0"),
                    new Version("0.0.0.0"), new Version("4.5.0.0"))
            }
        };
        _resolver = new FakeAssemblyResolver();
        _reader = new FakeAssemblyReader();
    }

    [Fact]
    public void Assemblies_Setter()
    {
#pragma warning disable IDE0017 // Simplify object initialization
        var context = GetFakeContext(new FrameworkConfig());
#pragma warning restore IDE0017 // Simplify object initialization
        context.Assemblies = new List<Assembly>();
    }

    [Fact]
    public void Find()
    {
        var context = GetFakeContext(new FrameworkConfig());

        var expected = AssHelp.Ass("TestAssembly");
        context.Add(expected);
        context.Add(AssHelp.Ass("TestAssembly", "1.2.3.4"));

        var actual = context.Find(AssHelp.Iden("TestAssembly"));
        Assert.NotNull(actual);
        Assert.Equal(expected.Version, actual!.Version);
    }

    [Fact]
    public void Find_UsesBindingRedirectAndCodeBase()
    {
        var context = GetFakeContext();
        context.Add(AssHelp.Ass("ReferenceA", "8.0.0.0") with
        {
            CodeBase = _config.CodeBases[0]
        }); // CodeBase resolved prior to find

        var actual = context.Find(AssHelp.Iden("ReferenceA", "3.0.0.0"));
        Assert.NotNull(actual);
        Assert.Equal(new Version("8.0.0.0"), actual!.Version);
    }


    [Fact]
    public void Resolve_WithoutCodeBase()
    {
        var context = GetFakeContext(new FrameworkConfig(), AssHelp.Ass("EntryPoint"));
        var actual = context.Resolve(AssHelp.Iden("ReferenceB", "2.5.0.0"));

        Assert.Equal("ReferenceB", actual.Name);
        Assert.Equal(new Version("2.5.0.0"), actual.Version);
    }

    [Fact]
    public void Resolve_UsesBindingRedirectAndCodeBase()
    {
        var context = GetFakeContext();
        var actual = context.Resolve(AssHelp.Iden("ReferenceA", "3.0.0.0"));

        Assert.Equal(AssemblySource.CodeBase, actual.Source);
        Assert.Equal(new Version("8.0.0.0"), actual.Version);
    }


    [Fact]
    public void Resolve_ResolverReturnedNullPath()
    {
        var context = GetFakeContext();
        Assert.Throws<Exception>(() => { context.Resolve(AssHelp.Iden("PathIsNull")); });
    }

    [Fact]
    public void Resolve_ResolverNotFound()
    {
        var context = GetFakeContext();
        var actual = context.Resolve(AssHelp.Iden("NotFound"));
        Assert.Equal(AssemblySource.NotFound, actual.Source);
    }

    [Fact]
    public void Contains()
    {
        var context = GetFakeContext();
        context.Add(AssHelp.Ass("Assembly"));
        Assert.True(context.Contains("Assembly"));
    }

    [Fact]
    public void Add_Duplicate()
    {
        var context = GetFakeContext();
        Assert.Throws<Exception>(() =>
        {
            context.Add(AssHelp.Ass("Duplicate"));
            context.Add(AssHelp.Ass("Duplicate"));
        });
    }

    [Fact]
    public void Add_DuplicateUnreferenced()
    {
        var context = GetFakeContext();
        context.Add(AssHelp.Ass("Duplicate") with { IsUnreferenced = true });
        context.Add(AssHelp.Ass("Duplicate") with { IsUnreferenced = true });
        context.Add(AssHelp.Ass("Duplicate") with { IsUnreferenced = true });


        // 4 including entry assembly
        Assert.Equal(4, context.Assemblies.Count);
    }

    private IContext GetFakeContext(FrameworkConfig? config = null, Assembly? entryPoint = null) =>
        new FrameworkContext(_resolver, _reader, null!, null!, config ?? _config, new List<TargetFramework>(),
            null!, entryPoint ?? AssHelp.Ass());

    private class FakeAssemblyResolver : IResolver
    {
        public IReadOnlyList<string> SearchDirectories => Array.Empty<string>();

        public AssemblyResolverResult ResolvePath(AssemblyIdentity identity)
        {
            if (identity.Name == "NotFound")
            {
                return new AssemblyResolverResult(AssemblySource.NotFound);
            }

            var path = identity.Name == "PathIsNull" ? null : identity.Name;
            return new AssemblyResolverResult(AssemblySource.Local, path);
        }

        public void AddSearchDirectory(string directory)
        {
            throw new NotSupportedException();
        }

        public void Dispose() { }
    }

    private class FakeAssemblyReader : IAssemblyReader
    {
        public Assembly Read(string fileName, AssemblySource source, bool applyMetadata = true)
        {
            return fileName switch
            {
                "EntryPoint" => AssHelp.Build("EntryPoint", "1.0.0.0", source,
                    new[] { ("ReferenceA", "3.0.0.0"), ("ReferenceB", "2.5.0.0") }),
                "ReferenceA" => AssHelp.Build("ReferenceA", "3.0.0.0", source),
                "ReferenceB" => AssHelp.Build("ReferenceB", "2.5.0.0", source),
                "ReferenceACodeBase.dll" => AssHelp.Build("ReferenceACodeBase", "8.0.0.0", source),
                _ => throw new ArgumentOutOfRangeException(nameof(fileName))
            };
        }

        public Assembly ReadOrDefault(
            string fileName,
            AssemblySource source,
            AssemblyIdentity defaultIdentity,
            bool applyMetadata = true) =>
            Read(fileName, source);
    }
}