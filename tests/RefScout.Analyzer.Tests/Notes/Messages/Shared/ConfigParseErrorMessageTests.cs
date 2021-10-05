using System;
using System.Collections.Generic;
using RefScout.Analyzer.Config;
using RefScout.Analyzer.Context;
using RefScout.Analyzer.Notes;
using RefScout.Analyzer.Notes.Messages;
using RefScout.Analyzer.Notes.Messages.Shared;
using RefScout.Analyzer.Tests.Fakes;
using Xunit;

namespace RefScout.Analyzer.Tests.Notes.Messages.Shared;

public class ConfigParseErrorMessageTests
{
    private readonly IContext _context;
    private readonly IContext _contextWithoutErrors;
    private readonly Message _generator;

    public ConfigParseErrorMessageTests()
    {
        _context = new FakeContext(new FakeConfig
        {
            ErrorReport = new FakeErrorReport
            {
                Errors = new[]
                {
                    new ConfigError("Hello", 123)
                }
            }
        });
        _contextWithoutErrors = new FakeContext(new FakeConfig
        {
            ErrorReport = new FakeErrorReport
            {
                Errors = new[]
                {
                    new ConfigError("Hello", 123)
                }
            }
        });
        _generator = new ConfigParseErrorMessage();
    }

    [Fact]
    public void Type_IsCorrect()
    {
        Assert.Equal(NoteType.LoadError, _generator.Type);
    }

    [Fact]
    public void Assembly_Test_True()
    {
        var assembly = AssHelp.Ass() with
        {
            IsEntryPoint = true
        };

        Assert.True(_generator.Test(_context, assembly));
        Assert.NotNull(_generator.Generate(_context, assembly));
    }

    [Fact]
    public void Assembly_Test_False()
    {
        var assembly = AssHelp.Ass() with
        {
            IsEntryPoint = true
        };

        Assert.True(_generator.Test(_contextWithoutErrors, assembly));
        Assert.NotNull(_generator.Generate(_contextWithoutErrors, assembly));
    }

    private class FakeConfig : IConfig
    {
        public IConfigErrorReport ErrorReport { get; set; } = new FakeErrorReport();
    }

    private class FakeErrorReport : IConfigErrorReport
    {
        public IReadOnlyList<ConfigError> Errors { get; init; } = Array.Empty<ConfigError>();
    }
}