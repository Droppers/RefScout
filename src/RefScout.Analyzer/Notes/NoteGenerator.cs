using System.Collections.Generic;
using System.Linq;
using RefScout.Analyzer.Context;
using RefScout.Analyzer.Helpers;
using RefScout.Analyzer.Notes.Messages;
using RefScout.Analyzer.Notes.Messages.Core;
using RefScout.Analyzer.Notes.Messages.Framework;
using RefScout.Analyzer.Notes.Messages.Mono;
using RefScout.Analyzer.Notes.Messages.Shared;
using RefScout.Analyzer.Notes.Messages.SharedFramework;

namespace RefScout.Analyzer.Notes;

internal class NoteGenerator : INoteGenerator
{
    private readonly IEnvironment _environment;

    private IReadOnlyList<Message>? _messages;

    public NoteGenerator(IEnvironment environment)
    {
        _environment = environment;
    }

    public void Generate(IContext context)
    {
        _messages ??= InitMessages();

        foreach (var generator in _messages)
        {
            foreach (var assembly in context.Assemblies)
            {
                foreach (var reference in assembly.ReferencedBy.Where(reference =>
                    generator.Test(context, reference)))
                {
                    reference.AddNote(generator.Type, generator.Generate(context, reference));
                }

                if (generator.Test(context, assembly))
                {
                    assembly.AddNote(generator.Type, generator.Generate(context, assembly));
                }
            }
        }
    }

    private IReadOnlyList<Message> InitMessages() => new List<Message>
    {
        new ConfigParseErrorMessage(),

        new CoreMissingRuntimeMessage(),
        new FrameworkMissingRuntimeMessage(),
        new MonoMissingRuntimeMessage(_environment),

        new UnreferencedMessage(),
        new ErrorMessage(),
        new LoadNotFoundMessage(),
        new ArchitectureMismatchMessage(),
        new RedirectFailedWrongVersionMessage(),
        new RedirectFailedMessage(),
        new RedirectSuccessMessage(),

        new FrameworkVersionMismatchWarningMessage(),
        new FrameworkVersionMismatchFatalMessage(),
        new CoreVersionMismatchWarningMessage(),

        new VersionMismatchMessage(),
        new UnificationMessage()
    };
}