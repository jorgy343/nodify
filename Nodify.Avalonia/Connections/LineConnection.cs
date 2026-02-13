using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;

namespace Nodify;

/// <summary>
/// Represents a line connection with optional corner radius.
/// </summary>
public class LineConnection : BaseConnection
{
    /// <summary>
    /// Defines the <see cref="CornerRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<double> CornerRadiusProperty =
        AvaloniaProperty.Register<LineConnection, double>(nameof(CornerRadius), defaultValue: 5.0);

    /// <summary>
    /// Gets or sets the radius of the corners between the line segments.
    /// </summary>
    public double CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    static LineConnection()
    {
        AffectsGeometry<LineConnection>(CornerRadiusProperty);
    }

    /// <summary>
    /// Draws the line connection geometry with optional smooth corners.
    /// </summary>
    protected override void DrawLineGeometry(StreamGeometryContext context, Point source, Point target)
    {
        var (p0, p1) = GetLinePoints(source, target);

        context.BeginFigure(source, false);

        if (CornerRadius > 0 && Spacing > 0)
        {
            AddSmoothCorner(context, source, p0, p1, CornerRadius);
            AddSmoothCorner(context, p0, p1, target, CornerRadius);
        }
        else
        {
            context.LineTo(p0);
            context.LineTo(p1);
        }

        context.LineTo(target);
        context.EndFigure(false);
    }

    /// <summary>
    /// Draws the arrowhead for line connections.
    /// </summary>
    protected override void DrawDefaultArrowhead(
        StreamGeometryContext context,
        Point source,
        Point target,
        ConnectionDirection arrowDirection = ConnectionDirection.Forward,
        Orientation orientation = Orientation.Horizontal)
    {
        if (Spacing < 1.0)
        {
            Vector delta = source - target;
            double headWidth = ArrowSize.Width;
            double headHeight = ArrowSize.Height / 2;

            double angle = Math.Atan2(delta.Y, delta.X);
            double sinT = Math.Sin(angle);
            double cosT = Math.Cos(angle);

            var from = new Point(
                target.X + (headWidth * cosT - headHeight * sinT),
                target.Y + (headWidth * sinT + headHeight * cosT));

            var to = new Point(
                target.X + (headWidth * cosT + headHeight * sinT),
                target.Y - (headHeight * cosT - headWidth * sinT));

            context.BeginFigure(target, true);
            context.LineTo(from);
            context.LineTo(to);
            context.EndFigure(true);
        }
        else
        {
            base.DrawDefaultArrowhead(context, source, target, arrowDirection, orientation);
        }
    }

    /// <summary>
    /// Calculates the intermediate points for the line connection.
    /// </summary>
    private (Point P0, Point P1) GetLinePoints(Point source, Point target)
    {
        double direction = Direction == ConnectionDirection.Forward ? 1.0 : -1.0;
        var spacing = new Vector(Spacing * direction, 0.0);
        var spacingVertical = new Vector(spacing.Y, spacing.X);

        var p0 = source + (SourceOrientation == Orientation.Vertical ? spacingVertical : spacing);
        var p1 = target - (TargetOrientation == Orientation.Vertical ? spacingVertical : spacing);

        return (p0, p1);
    }

    /// <summary>
    /// Interpolates a point on a line segment at parameter t (0 to 1).
    /// </summary>
    protected static Point InterpolateLineSegment(Point p0, Point p1, double t)
    {
        return new Point(
            (1 - t) * p0.X + t * p1.X,
            (1 - t) * p0.Y + t * p1.Y);
    }

    /// <summary>
    /// Adds a smooth corner using a quadratic bezier curve.
    /// </summary>
    protected static void AddSmoothCorner(
        StreamGeometryContext context,
        Point start,
        Point corner,
        Point end,
        double radius)
    {
        Vector vecAB = corner - start;
        Vector vecBC = end - corner;

        double distAB = vecAB.Length * vecAB.Length;
        double distBC = vecBC.Length * vecBC.Length;

        double bendSize = Math.Sqrt(Math.Min(distAB, distBC)) / 2;
        radius = Math.Min(bendSize, radius);

        Vector directionToCorner = corner - start;
        Vector directionFromCorner = end - corner;

        double lenToCorner = directionToCorner.Length;
        double lenFromCorner = directionFromCorner.Length;

        if (lenToCorner != 0)
        {
            directionToCorner = directionToCorner.Normalize();
        }

        if (lenFromCorner != 0)
        {
            directionFromCorner = directionFromCorner.Normalize();
        }

        Point curveStart = corner - directionToCorner * radius;
        Point curveEnd = corner + directionFromCorner * radius;

        context.LineTo(curveStart);
        context.QuadraticBezierTo(corner, curveEnd);
    }
}
