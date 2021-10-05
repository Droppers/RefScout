namespace RefScout.Visualizers.Dot.Compiler;

internal class DotNode : DotElement
{
    private readonly string _id;

    public DotNode(string id)
    {
        _id = id;
    }

    public DotNode(int fromId) : this(fromId.ToString()) { }

    protected override string Prefix() => DotHelpers.Quote(_id);
}