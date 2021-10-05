using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using RefScout.Analyzer.Analyzers.Compatibility;
using RefScout.Analyzer.Analyzers.Environment;
using RefScout.Analyzer.Analyzers.Environment.Core;
using RefScout.Analyzer.Analyzers.Environment.Mono;
using RefScout.Analyzer.Config.Framework;
using RefScout.Analyzer.Context;
using RefScout.Analyzer.Readers;
using RefScout.Analyzer.Resolvers;
using Xunit;

namespace RefScout.Analyzer.Tests.Analyzers.Compatibility;

public class FrameworkCompatibilityAnalyzerTests
{
    private static readonly PublicKeyToken SamplePublicKeyToken = PublicKeyToken.Parse("969db8053d3322ac");

    [Fact]
    public void Run_BindingRedirectStatus()
    {
        var config = new FrameworkConfig
        {
            BindingRedirects = new[]
            {
                new BindingRedirect(new BindingIdentity("ReferenceA", "neutral", PublicKeyToken.Empty),
                    new Version("5.0.0.0"),
                    new Version("0.0.0.0"), new Version("2.0.0.0")),
                new BindingRedirect(new BindingIdentity("ReferenceC", "neutral", PublicKeyToken.Empty),
                    new Version("12.0.0.0"),
                    new Version("0.0.0.0"), new Version("10.0.0.0"))
            }
        };

        var referenceA = AssHelp.Ass("ReferenceA", "5.0.0.0");
        var referenceB = AssHelp.Ass("ReferenceB");
        var entryPoint = AssHelp.Ass("EntryPoint");
        var referenceC = AssHelp.Ass("ReferenceC", "12.0.0.32", SamplePublicKeyToken);

        entryPoint.References.Add(AssHelp.Ref(entryPoint, referenceA, "3.5.0.0"));
        entryPoint.References.Add(AssHelp.Ref(entryPoint, referenceB, "2.5.0.0"));
        entryPoint.References.Add(AssHelp.Ref(entryPoint, referenceC, "5.0.0.0"));
        referenceB.References.Add(AssHelp.Ref(referenceB, referenceA, "1.5.0.0"));

        var context = new FrameworkContext(new Mock<IResolver>().Object, new Mock<IAssemblyReader>().Object,
            new EnvironmentInfo(new CoreRuntimeAnalyzerResult(null!), null, new MonoRuntimeAnalyzerResult(null!)),
            new FrameworkConfig(), config, new List<TargetFramework>(), null!,
            entryPoint)
        {
            Assemblies = new[] { entryPoint, referenceA, referenceB }
        };

        var step = new SharedFrameworkCompatibilityAnalyzer(context,
            new DefaultVersionComparer(VersionCompatibilityMode.Loose));
        step.Analyze();

        var expectDefault = context.Assemblies.Single(x => x.Name == "EntryPoint").References
            .Single(r => r.To.Name == "ReferenceB");
        var expectFail = context.Assemblies.Single(x => x.Name == "EntryPoint").References
            .Single(r => r.To.Name == "ReferenceA");
        var expectSuccess = context.Assemblies.Single(x => x.Name == "ReferenceB").References
            .Single(r => r.To.Name == "ReferenceA");
        var expectFailWrongVersion = context.Assemblies.Single(x => x.Name == "EntryPoint").References
            .Single(r => r.To.Name == "ReferenceC");

        Assert.Equal(BindingRedirectStatus.Default, expectDefault.BindingRedirectStatus);
        Assert.Equal(BindingRedirectStatus.Failed, expectFail.BindingRedirectStatus);
        Assert.Equal(BindingRedirectStatus.Success, expectSuccess.BindingRedirectStatus);
        Assert.Equal(BindingRedirectStatus.FailedWrongVersion, expectFailWrongVersion.BindingRedirectStatus);
    }
}