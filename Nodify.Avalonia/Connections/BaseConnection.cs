using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Windows.Input;

namespace Nodify;

/// <summary>
/// Represents the base class for shapes that are drawn from a <see cref="Source"/> point to a <see cref="Target"/> point.
/// This is the Avalonia port of the WPF BaseConnection control with modernized API.
/// </summary>
public abstract class BaseConnection : Shape
{
    #region Styled Properties

    /// <summary>
    /// Defines the <see cref="Source"/> property.
    /// </summary>
    public static readonly StyledProperty<Point> SourceProperty =
        AvaloniaProperty.Register<BaseConnection, Point>(nameof(Source), defaultValue: default);

    /// <summary>
    /// Defines the <see cref="Target"/> property.
    /// </summary>
    public static readonly StyledProperty<Point> TargetProperty =
        AvaloniaProperty.Register<BaseConnection, Point>(nameof(Target), defaultValue: default);

    /// <summary>
    /// Defines the <see cref="SourceOffset"/> property.
    /// </summary>
    public static readonly StyledProperty<Size> SourceOffsetProperty =
        AvaloniaProperty.Register<BaseConnection, Size>(nameof(SourceOffset), new Size(14, 0));

    /// <summary>
    /// Defines the <see cref="TargetOffset"/> property.
    /// </summary>
    public static readonly StyledProperty<Size> TargetOffsetProperty =
        AvaloniaProperty.Register<BaseConnection, Size>(nameof(TargetOffset), new Size(14, 0));

    /// <summary>
    /// Defines the <see cref="SourceOffsetMode"/> property.
    /// </summary>
    public static readonly StyledProperty<ConnectionOffsetMode> SourceOffsetModeProperty =
        AvaloniaProperty.Register<BaseConnection, ConnectionOffsetMode>(
            nameof(SourceOffsetMode),
            ConnectionOffsetMode.Static);

    /// <summary>
    /// Defines the <see cref="TargetOffsetMode"/> property.
    /// </summary>
    public static readonly StyledProperty<ConnectionOffsetMode> TargetOffsetModeProperty =
        AvaloniaProperty.Register<BaseConnection, ConnectionOffsetMode>(
            nameof(TargetOffsetMode),
            ConnectionOffsetMode.Static);

    /// <summary>
    /// Defines the <see cref="SourceOrientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> SourceOrientationProperty =
        AvaloniaProperty.Register<BaseConnection, Orientation>(
            nameof(SourceOrientation),
            Orientation.Horizontal);

    /// <summary>
    /// Defines the <see cref="TargetOrientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> TargetOrientationProperty =
        AvaloniaProperty.Register<BaseConnection, Orientation>(
            nameof(TargetOrientation),
            Orientation.Horizontal);

    /// <summary>
    /// Defines the <see cref="Direction"/> property.
    /// </summary>
    public static readonly StyledProperty<ConnectionDirection> DirectionProperty =
        AvaloniaProperty.Register<BaseConnection, ConnectionDirection>(
            nameof(Direction),
            ConnectionDirection.Forward);

    /// <summary>
    /// Defines the <see cref="Spacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<BaseConnection, double>(nameof(Spacing), defaultValue: 0.0);

    /// <summary>
    /// Defines the <see cref="ArrowSize"/> property.
    /// </summary>
    public static readonly StyledProperty<Size> ArrowSizeProperty =
        AvaloniaProperty.Register<BaseConnection, Size>(nameof(ArrowSize), new Size(8, 8));

    /// <summary>
    /// Defines the <see cref="ArrowEnds"/> property.
    /// </summary>
    public static readonly StyledProperty<ArrowHeadEnds> ArrowEndsProperty =
        AvaloniaProperty.Register<BaseConnection, ArrowHeadEnds>(
            nameof(ArrowEnds),
            ArrowHeadEnds.End);

    /// <summary>
    /// Defines the <see cref="ArrowShape"/> property.
    /// </summary>
    public static readonly StyledProperty<ArrowHeadShape> ArrowShapeProperty =
        AvaloniaProperty.Register<BaseConnection, ArrowHeadShape>(
            nameof(ArrowShape),
            ArrowHeadShape.Arrowhead);

    /// <summary>
    /// Defines the <see cref="SplitCommand"/> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> SplitCommandProperty =
        AvaloniaProperty.Register<BaseConnection, ICommand?>(nameof(SplitCommand));

    /// <summary>
    /// Defines the <see cref="DisconnectCommand"/> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> DisconnectCommandProperty =
        AvaloniaProperty.Register<BaseConnection, ICommand?>(nameof(DisconnectCommand));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the start point of the connection.
    /// </summary>
    public Point Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the end point of the connection.
    /// </summary>
    public Point Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>
    /// Gets or sets the offset from the <see cref="Source"/> point.
    /// </summary>
    public Size SourceOffset
    {
        get => GetValue(SourceOffsetProperty);
        set => SetValue(SourceOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets the offset from the <see cref="Target"/> point.
    /// </summary>
    public Size TargetOffset
    {
        get => GetValue(TargetOffsetProperty);
        set => SetValue(TargetOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets the offset mode for the source.
    /// </summary>
    public ConnectionOffsetMode SourceOffsetMode
    {
        get => GetValue(SourceOffsetModeProperty);
        set => SetValue(SourceOffsetModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the offset mode for the target.
    /// </summary>
    public ConnectionOffsetMode TargetOffsetMode
    {
        get => GetValue(TargetOffsetModeProperty);
        set => SetValue(TargetOffsetModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the orientation of the source connector.
    /// </summary>
    public Orientation SourceOrientation
    {
        get => GetValue(SourceOrientationProperty);
        set => SetValue(SourceOrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the orientation of the target connector.
    /// </summary>
    public Orientation TargetOrientation
    {
        get => GetValue(TargetOrientationProperty);
        set => SetValue(TargetOrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the direction of the connection.
    /// </summary>
    public ConnectionDirection Direction
    {
        get => GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    /// <summary>
    /// Gets or sets the spacing between the connector and the start of the connection line.
    /// </summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the size of the arrowhead.
    /// </summary>
    public Size ArrowSize
    {
        get => GetValue(ArrowSizeProperty);
        set => SetValue(ArrowSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets which end of the connection has an arrowhead.
    /// </summary>
    public ArrowHeadEnds ArrowEnds
    {
        get => GetValue(ArrowEndsProperty);
        set => SetValue(ArrowEndsProperty, value);
    }

    /// <summary>
    /// Gets or sets the shape of the arrowhead.
    /// </summary>
    public ArrowHeadShape ArrowShape
    {
        get => GetValue(ArrowShapeProperty);
        set => SetValue(ArrowShapeProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to split the connection.
    /// </summary>
    public ICommand? SplitCommand
    {
        get => GetValue(SplitCommandProperty);
        set => SetValue(SplitCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to disconnect the connection.
    /// </summary>
    public ICommand? DisconnectCommand
    {
        get => GetValue(DisconnectCommandProperty);
        set => SetValue(DisconnectCommandProperty, value);
    }

    #endregion

    static BaseConnection()
    {
        AffectsGeometry<BaseConnection>(
            SourceProperty,
            TargetProperty,
            SourceOffsetProperty,
            TargetOffsetProperty,
            SourceOffsetModeProperty,
            TargetOffsetModeProperty,
            SourceOrientationProperty,
            TargetOrientationProperty,
            DirectionProperty,
            SpacingProperty,
            ArrowSizeProperty,
            ArrowEndsProperty,
            ArrowShapeProperty);
    }

    /// <summary>
    /// Creates the geometry for the connection. Override this to customize the connection shape.
    /// </summary>
    protected override Geometry? CreateDefiningGeometry()
    {
        var geometry = new StreamGeometry();

        using (var context = geometry.Open())
        {
            // Apply offsets based on offset mode
            Point source = ApplyOffset(Source, SourceOffset, SourceOffsetMode, SourceOrientation);
            Point target = ApplyOffset(Target, TargetOffset, TargetOffsetMode, TargetOrientation);

            // Draw the connection line
            DrawLineGeometry(context, source, target);
        }

        return geometry;
    }

    /// <summary>
    /// Draws the line geometry from source to target. Override this to customize the line shape.
    /// </summary>
    protected virtual void DrawLineGeometry(StreamGeometryContext context, Point source, Point target)
    {
        // Default implementation: straight line
        context.BeginFigure(source, false);
        context.LineTo(target);
        context.EndFigure(false);
    }

    /// <summary>
    /// Applies the offset to a point based on the offset mode.
    /// </summary>
    private Point ApplyOffset(Point point, Size offset, ConnectionOffsetMode mode, Orientation orientation)
    {
        switch (mode)
        {
            case ConnectionOffsetMode.Static:
                return point + (orientation == Orientation.Horizontal
                    ? new Vector(offset.Width, offset.Height)
                    : new Vector(offset.Height, offset.Width));

            case ConnectionOffsetMode.Circle:
            case ConnectionOffsetMode.Rectangle:
            case ConnectionOffsetMode.Edge:
                // Simplified implementation - can be enhanced later
                return point + new Vector(offset.Width, offset.Height);

            case ConnectionOffsetMode.None:
            default:
                return point;
        }
    }

    /// <summary>
    /// Draws the default arrowhead at the specified position.
    /// </summary>
    protected virtual void DrawDefaultArrowhead(
        StreamGeometryContext context,
        Point source,
        Point target,
        ConnectionDirection arrowDirection = ConnectionDirection.Forward,
        Orientation orientation = Orientation.Horizontal)
    {
        Vector delta = target - source;
        double headWidth = ArrowSize.Width;
        double headHeight = ArrowSize.Height / 2;

        if (orientation == Orientation.Horizontal)
        {
            double direction = arrowDirection == ConnectionDirection.Forward ? 1 : -1;
            Point tipPoint = target;
            Point baseLeft = new Point(target.X - headWidth * direction, target.Y - headHeight);
            Point baseRight = new Point(target.X - headWidth * direction, target.Y + headHeight);

            context.BeginFigure(tipPoint, true);
            context.LineTo(baseLeft);
            context.LineTo(baseRight);
            context.EndFigure(true);
        }
    }
}
