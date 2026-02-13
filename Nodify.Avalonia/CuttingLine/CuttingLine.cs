using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace Nodify;

public class CuttingLine : Control
{
    public static readonly StyledProperty<Point> StartPointProperty =
        AvaloniaProperty.Register<CuttingLine, Point>(
            nameof(StartPoint),
            defaultValue: default,
            inherits: false,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<Point> EndPointProperty =
        AvaloniaProperty.Register<CuttingLine, Point>(
            nameof(EndPoint),
            defaultValue: default,
            inherits: false,
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<CuttingLine, IBrush?>(nameof(Stroke));

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<CuttingLine, double>(nameof(StrokeThickness), defaultValue: 1.0);

    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<CuttingLine, IBrush?>(nameof(Fill));

    /// <summary>
    /// Will be set for <see cref="BaseConnection"/>s and custom connections when the cutting line intersects with them if <see cref="NodifyEditor.EnableCuttingLinePreview"/> is true.
    /// </summary>
    public static readonly AttachedProperty<bool> IsOverElementProperty =
        AvaloniaProperty.RegisterAttached<CuttingLine, Control, bool>("IsOverElement");

    public static bool GetIsOverElement(Control elem)
        => elem.GetValue(IsOverElementProperty);

    public static void SetIsOverElement(Control elem, bool value)
        => elem.SetValue(IsOverElementProperty, value);

    /// <summary>
    /// Gets or sets the start point.
    /// </summary>
    public Point StartPoint
    {
        get => GetValue(StartPointProperty);
        set => SetValue(StartPointProperty, value);
    }

    /// <summary>
    /// Gets or sets the end point.
    /// </summary>
    public Point EndPoint
    {
        get => GetValue(EndPointProperty);
        set => SetValue(EndPointProperty, value);
    }

    /// <summary>
    /// Gets or sets the stroke brush for the line.
    /// </summary>
    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    /// <summary>
    /// Gets or sets the stroke thickness.
    /// </summary>
    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the fill brush for the ellipses at the endpoints.
    /// </summary>
    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    static CuttingLine()
    {
        AffectsRender<CuttingLine>(StartPointProperty, EndPointProperty, StrokeProperty, StrokeThicknessProperty, FillProperty);
        IsHitTestVisibleProperty.OverrideDefaultValue<CuttingLine>(false);
        IsEnabledProperty.OverrideDefaultValue<CuttingLine>(false);
    }

    public override void Render(DrawingContext drawingContext)
    {
        base.Render(drawingContext);

        var stroke = Stroke;
        if (stroke == null)
            return;

        var pen = new Pen(stroke, StrokeThickness);
        var fill = Fill;

        // Draw the line
        drawingContext.DrawLine(pen, StartPoint, EndPoint);

        // Draw ellipses at start and end points
        var radius = StrokeThickness * 1.2;
        if (fill != null)
        {
            drawingContext.DrawEllipse(fill, null, StartPoint, radius, radius);
            drawingContext.DrawEllipse(fill, null, EndPoint, radius, radius);
        }
    }
}
