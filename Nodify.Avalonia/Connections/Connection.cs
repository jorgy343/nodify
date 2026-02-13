using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;

namespace Nodify;

/// <summary>
/// Represents a cubic bezier curve connection.
/// </summary>
public class Connection : BaseConnection
{
    private const double _baseOffset = 100.0;
    private const double _offsetGrowthRate = 25.0;

    /// <summary>
    /// Draws the cubic bezier curve geometry.
    /// </summary>
    protected override void DrawLineGeometry(StreamGeometryContext context, Point source, Point target)
    {
        var (p0, p1, p2, p3) = GetBezierControlPoints(source, target);

        context.BeginFigure(source, false);
        context.LineTo(p0);
        context.CubicBezierTo(p1, p2, p3);
        context.LineTo(target);
        context.EndFigure(false);
    }

    /// <summary>
    /// Calculates the control points for the cubic bezier curve.
    /// </summary>
    private (Point P0, Point P1, Point P2, Point P3) GetBezierControlPoints(Point source, Point target)
    {
        double direction = Direction == ConnectionDirection.Forward ? 1.0 : -1.0;
        var spacing = new Vector(Spacing * direction, 0.0);
        var spacingVertical = new Vector(spacing.Y, spacing.X);

        Point startPoint = source + (SourceOrientation == Orientation.Vertical ? spacingVertical : spacing);
        Point endPoint = target - (TargetOrientation == Orientation.Vertical ? spacingVertical : spacing);

        Vector delta = target - source;
        double height = Math.Abs(delta.Y);
        double width = Math.Abs(delta.X);

        // Smooth curve when distance is lower than base offset
        double smooth = Math.Min(_baseOffset, height);
        // Calculate offset based on distance
        double offset = Math.Max(smooth, width / 2.0);
        // Grow slowly with distance
        offset = Math.Min(_baseOffset + Math.Sqrt(width * _offsetGrowthRate), offset);

        var controlPoint = new Vector(offset * direction, 0.0);
        var controlPointVertical = new Vector(controlPoint.Y, controlPoint.X);

        // Avoid sharp bend if orientation different (when close to each other)
        if (TargetOrientation != SourceOrientation)
        {
            controlPoint *= 0.5;
        }

        Point p0 = startPoint;
        Point p1 = startPoint + (SourceOrientation == Orientation.Vertical ? controlPointVertical : controlPoint);
        Point p2 = endPoint - (TargetOrientation == Orientation.Vertical ? controlPointVertical : controlPoint);
        Point p3 = endPoint;

        return (p0, p1, p2, p3);
    }

    /// <summary>
    /// Interpolates a point on the cubic bezier curve at parameter t (0 to 1).
    /// </summary>
    protected static Point InterpolateCubicBezier(Point p0, Point p1, Point p2, Point p3, double t)
    {
        // B = (1 − t)^3 * P0 + 3 * t * (1 − t)^2 * P1 + 3 * t^2 * (1 − t) * P2 + t^3 * P3
        return new Point(
            (1 - t) * (1 - t) * (1 - t) * p0.X
            + 3 * t * (1 - t) * (1 - t) * p1.X
            + 3 * t * t * (1 - t) * p2.X
            + t * t * t * p3.X,
            (1 - t) * (1 - t) * (1 - t) * p0.Y
            + 3 * t * (1 - t) * (1 - t) * p1.Y
            + 3 * t * t * (1 - t) * p2.Y
            + t * t * t * p3.Y);
    }

    /// <summary>
    /// Calculates the tangent vector at parameter t on the bezier curve.
    /// </summary>
    private static Vector GetBezierTangent(Point p0, Point p1, Point p2, Point p3, double t)
    {
        // Calculate the derivatives of the Bezier curve equation and negate the result
        double dx = -3 * (1 - t) * (1 - t) * p0.X
                  + (3 * (1 - t) * (1 - t) - 6 * t * (1 - t)) * p1.X
                  + (6 * t * (1 - t) - 3 * t * t) * p2.X
                  + 3 * t * t * p3.X;

        double dy = -3 * (1 - t) * (1 - t) * p0.Y
                  + (3 * (1 - t) * (1 - t) - 6 * t * (1 - t)) * p1.Y
                  + (6 * t * (1 - t) - 3 * t * t) * p2.Y
                  + 3 * t * t * p3.Y;

        return new Vector(-dx, -dy);
    }
}
