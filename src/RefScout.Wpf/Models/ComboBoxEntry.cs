namespace RefScout.Wpf.Models;

internal record ComboBoxEntry<T>(T Value, string Description)
{
    public override string ToString() => Description;
}