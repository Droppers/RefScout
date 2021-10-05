using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RefScout.Core.Logging;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace RefScout.Wpf.Views.Controls;

public partial class GraphViewer : UserControl
{
    private const double ZoomChange = 0.1;

    public static readonly DependencyProperty VectorPathProperty =
        DependencyProperty.Register("VectorPath",
            typeof(string),
            typeof(GraphViewer),
            new FrameworkPropertyMetadata(string.Empty, OnVectorPathChanged));

    private WpfDrawingDocument? _drawingDocument;
    private MouseButton _mouseButtonDown;
    private MouseHandlingMode _mouseHandlingMode;
    private bool _mouseMoved;
    private Point _origContentMouseDownPoint;

    public GraphViewer()
    {
        InitializeComponent();
    }

    public string VectorPath
    {
        get => (string)GetValue(VectorPathProperty);
        set => SetValue(VectorPathProperty, value);
    }

    public double FitZoomValue
    {
        get
        {
            if (ZoomPanControl == null)
            {
                return 1;
            }

            var content = ZoomPanControl.ContentElement;
            return FitZoom(Container.ActualWidth - SystemParameters.VerticalScrollBarWidth - 2,
                Container.ActualHeight - SystemParameters.HorizontalScrollBarHeight - 2,
                content?.ActualWidth, content?.ActualHeight);
        }
    }

    public event EventHandler<string>? HitNodeClicked;
    public event EventHandler? GraphRendered;
    public event EventHandler<Exception>? GraphRenderFailed;

    private static async void OnVectorPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var viewer = (GraphViewer)d;
        await viewer.RenderSync((string)e.NewValue);
    }

    private double FitZoom(
        double actualWidth,
        double actualHeight,
        double? contentWidth,
        double? contentHeight)
    {
        if (!contentWidth.HasValue || !contentHeight.HasValue)
        {
            return 1;
        }

        return Math.Min(Math.Min(actualWidth / contentWidth.Value, actualHeight / contentHeight.Value),
            Math.Min(Container.ActualWidth, Container.ActualHeight) / 300);
    }

    private async Task RenderSync(string vectorPath)
    {
        try
        {
            await LoadFileAsync(vectorPath);
            GraphRendered?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception e)
        {
            GraphRenderFailed?.Invoke(this, e);
        }
    }

    private void OnZoomPanMouseDown(object sender, MouseButtonEventArgs e)
    {
        _mouseMoved = false;

        ZoomPanControl.Focus();
        Keyboard.Focus(ZoomPanControl);

        _mouseButtonDown = e.ChangedButton;
        _origContentMouseDownPoint = e.GetPosition(SvgViewer);

        if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 &&
            e.ChangedButton is MouseButton.Left or MouseButton.Right)
        {
            _mouseHandlingMode = MouseHandlingMode.Zooming;
        }
        else if (_mouseButtonDown == MouseButton.Left)
        {
            _mouseHandlingMode = MouseHandlingMode.Panning;
        }
        else if (_mouseHandlingMode != MouseHandlingMode.None)
        {
            ZoomPanControl.CaptureMouse();
            e.Handled = true;
        }
    }

    private void OnZoomPanMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_mouseHandlingMode == MouseHandlingMode.Panning && !_mouseMoved &&
            _mouseButtonDown == MouseButton.Left && _drawingDocument != null)
        {
            var point = e.GetPosition(SvgViewer);
            _drawingDocument.DisplayTransform = SvgViewer.DisplayTransform;
            var hitResult = _drawingDocument.HitTest(point);

            var element = hitResult.Element;
            if (element?.Id?.StartsWith("hit-", StringComparison.OrdinalIgnoreCase) == true)
            {
                var id = element.Id[4..];
                HitNodeClicked?.Invoke(this, id);
            }
        }

        switch (_mouseHandlingMode)
        {
            case MouseHandlingMode.None:
                return;
            case MouseHandlingMode.Zooming when _mouseButtonDown == MouseButton.Left:
                // Shift + left-click zooms in on the content.
                ZoomIn(_origContentMouseDownPoint);
                break;
            case MouseHandlingMode.Zooming when _mouseButtonDown == MouseButton.Right:
                ZoomOut(_origContentMouseDownPoint);
                break;
        }

        ZoomPanControl.ReleaseMouseCapture();
        _mouseHandlingMode = MouseHandlingMode.None;
        e.Handled = true;
    }

    private void OnZoomPanMouseMove(object sender, MouseEventArgs e)
    {
        _mouseMoved = true;
        if (_mouseHandlingMode != MouseHandlingMode.Panning)
        {
            return;
        }

        e.Handled = true;

        var curContentMousePoint = e.GetPosition(SvgViewer);
        var dragOffset = curContentMousePoint - _origContentMouseDownPoint;

        ZoomPanControl.ContentOffsetX -= dragOffset.X;
        ZoomPanControl.ContentOffsetY -= dragOffset.Y;
    }

    private void OnZoomPanMouseWheel(object sender, MouseWheelEventArgs e)
    {
        e.Handled = true;

        var curContentMousePoint = e.GetPosition(SvgViewer);
        Zoom(curContentMousePoint, e.Delta);

        if (SvgViewer.IsKeyboardFocusWithin)
        {
            Keyboard.Focus(ZoomPanControl);
        }
    }

    private void Zoom(Point contentZoomCenter, int wheelMouseDelta)
    {
        var zoomFactor = ZoomPanControl.ContentScale + ZoomChange * wheelMouseDelta / (120 * 3);
        ZoomPanControl.ZoomAboutPoint(zoomFactor, contentZoomCenter);
        UpdateCurrentZoom(zoomFactor);
    }

    private void OnZoomIn(object sender, RoutedEventArgs e)
    {
        ZoomIn(new Point(ZoomPanControl.ContentZoomFocusX, ZoomPanControl.ContentZoomFocusY));
    }

    private void OnZoomOut(object sender, RoutedEventArgs e)
    {
        ZoomOut(new Point(ZoomPanControl.ContentZoomFocusX, ZoomPanControl.ContentZoomFocusY));
    }

    private void OnZoomFit(object sender, RoutedEventArgs e)
    {
        ZoomPanControl.AnimatedZoomTo(FitZoomValue);
        UpdateCurrentZoom(FitZoomValue);
    }

    private void ZoomOut(Point contentZoomCenter)
    {
        var zoom = ZoomPanControl.ContentScale - ZoomChange;
        ZoomPanControl.ZoomAboutPoint(zoom, contentZoomCenter);
        UpdateCurrentZoom(zoom);
    }

    private void ZoomIn(Point contentZoomCenter)
    {
        var zoom = ZoomPanControl.ContentScale + ZoomChange;
        ZoomPanControl.ZoomAboutPoint(zoom, contentZoomCenter);
        UpdateCurrentZoom(zoom);
    }

    private void UpdateCurrentZoom(double zoom, double? fitScale = null)
    {
        var startZoom = fitScale ?? FitZoomValue;
        var percentage = Math.Round(zoom / startZoom * 100);
        CurrentZoom.Text = $"{percentage}%";
    }

    private async Task LoadFileAsync(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        _drawingDocument = null;
        using var fileReader = new FileSvgReader(new WpfDrawingSettings()) { SaveXaml = false, SaveZaml = false };
        DrawingGroup drawing;
        try
        {
            drawing = await Task.Run(() =>
            {
                var d = fileReader.Read(new Uri(fileName));
                d.Freeze();
                return d;
            });
        }
        catch (Exception e)
        {
            Logger.Error(e, "Could not render SVG output from GraphViz");
            SvgViewer.UnloadDiagrams();
            GraphRenderFailed?.Invoke(this, e);
            return;
        }
        finally
        {
            fileReader.Dispose();
        }

        _drawingDocument = fileReader.DrawingDocument;
        SvgViewer.UnloadDiagrams();
        SvgViewer.RenderDiagrams(drawing);

        ZoomPanControl.InvalidateMeasure();

        var fitZoom = FitZoom(Container.ActualWidth - SystemParameters.VerticalScrollBarWidth - 2,
            Container.ActualHeight - SystemParameters.HorizontalScrollBarHeight - 2,
            drawing.Bounds.Width, drawing.Bounds.Height);
        ZoomPanControl.ZoomTo(fitZoom);
        UpdateCurrentZoom(fitZoom, fitZoom);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateCurrentZoom(ZoomPanControl.ContentScale);
    }
}

public enum MouseHandlingMode
{
    None,
    DraggingRectangles,
    Panning,
    Zooming
}