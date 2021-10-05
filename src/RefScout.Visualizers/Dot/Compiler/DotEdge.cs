namespace RefScout.Visualizers.Dot.Compiler;

internal class DotEdge : DotElement
{
    private readonly string _fromId;
    private readonly string _toId;

    public DotEdge(string fromId, string toId)
    {
        _fromId = fromId;
        _toId = toId;
    }

    public DotEdge(int fromId, int toId) : this(fromId.ToString(), toId.ToString()) { }
    public DotEdge(string fromId, int toId) : this(fromId, toId.ToString()) { }
    public DotEdge(int fromId, string toId) : this(fromId.ToString(), toId) { }

    protected override string Prefix() => $"{DotHelpers.Quote(_fromId)} -> {DotHelpers.Quote(_toId)}";
}