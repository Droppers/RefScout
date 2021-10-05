using RefScout.Analyzer.Context;

namespace RefScout.Analyzer.Notes.Messages;

internal interface IMessage<in T>
{
    bool Test(IContext context, T t);
    string Generate(IContext context, T t);
}