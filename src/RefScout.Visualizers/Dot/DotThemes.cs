using System.Drawing;

namespace RefScout.Visualizers.Dot;

internal interface IDotTheme
{
    Color Background { get; }

    Color FontColor { get; }

    Color NodeFill { get; }
    Color NodeBorder { get; }

    Color NodeFatal { get; }
    Color NodeInfo { get; }
    Color NodeWarning { get; }
    Color NodeSuccess { get; }

    Color NodeGac { get; }

    // Edges
    Color EdgeStroke { get; }
    Color EdgeFatal { get; }
    Color EdgeInfo { get; }
    Color EdgeWarning { get; }
    Color EdgeSuccess { get; }

    Color NodeFramework { get; }
    Color NodeFrameworkText { get; }
    Color EdgeFramework { get; }
}

internal class DotLightTheme : IDotTheme
{
    public Color Background => Color.White;

    public Color FontColor => Color.FromArgb(33, 33, 33);

    public Color NodeFill => Color.White;
    public Color NodeBorder => Color.FromArgb(50, 50, 50);

    public Color NodeGac => Color.FromArgb(234, 123, 247);
    public Color NodeFatal => Color.FromArgb(239, 83, 80);
    public Color NodeInfo => Color.FromArgb(0, 162, 232);
    public Color NodeWarning => Color.FromArgb(245, 124, 0);
    public Color NodeSuccess => Color.FromArgb(102, 187, 106);

    public Color EdgeStroke => Color.FromArgb(33, 33, 33);
    public Color EdgeFatal => Color.FromArgb(239, 83, 80);
    public Color EdgeInfo => Color.FromArgb(0, 162, 232);
    public Color EdgeWarning => Color.FromArgb(245, 124, 0);
    public Color EdgeSuccess => Color.FromArgb(102, 187, 106);

    public Color NodeFramework => Color.FromArgb(101, 42, 119);
    public Color NodeFrameworkText => Color.White;
    public Color EdgeFramework => Color.DimGray;
}

internal class DotDarkTheme : IDotTheme
{
    public Color Background => Color.FromArgb(36, 36, 36);

    public Color FontColor => Color.FromArgb(230, 230, 230);

    public Color NodeFill => Color.FromArgb(43, 43, 43);
    public Color NodeBorder => Color.FromArgb(70, 70, 70);

    public Color NodeGac => Color.FromArgb(234, 123, 247);
    public Color NodeFatal => Color.FromArgb(219, 56, 50);
    public Color NodeInfo => Color.FromArgb(0, 162, 232);
    public Color NodeWarning => Color.FromArgb(245, 124, 0);
    public Color NodeSuccess => Color.FromArgb(59, 160, 64);

    public Color EdgeStroke => Color.FromArgb(170, 170, 170);
    public Color EdgeFatal => Color.FromArgb(219, 56, 50);
    public Color EdgeInfo => Color.FromArgb(0, 162, 232);
    public Color EdgeWarning => Color.FromArgb(245, 124, 0);
    public Color EdgeSuccess => Color.FromArgb(59, 160, 64);

    public Color NodeFramework => Color.FromArgb(101, 42, 119);
    public Color NodeFrameworkText => Color.White;
    public Color EdgeFramework => Color.FromArgb(100, 100, 100);
}