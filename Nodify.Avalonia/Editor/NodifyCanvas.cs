using Avalonia;
using Avalonia.Controls;

namespace Nodify;

/// <summary>
/// Interface for items inside a <see cref="NodifyCanvas"/>.
/// </summary>
public interface INodifyCanvasItem
{
    /// <summary>The location of the item.</summary>
    Point Location { get; }

    /// <summary>The desired size of the item.</summary>
    Size DesiredSize { get; }

    /// <summary>Arranges the item within the specified rectangle.</summary>
    void Arrange(Rect rect);
}

/// <summary>
/// A canvas-like panel that works with <see cref="INodifyCanvasItem"/>s.
/// </summary>
public class NodifyCanvas : Panel
{
    /// <summary>
    /// Defines the <see cref="Extent"/> property.
    /// </summary>
    public static readonly StyledProperty<Rect> ExtentProperty =
        AvaloniaProperty.Register<NodifyCanvas, Rect>(nameof(Extent), default(Rect));

    /// <summary>The area covered by the children of this panel.</summary>
    public Rect Extent
    {
        get => GetValue(ExtentProperty);
        set => SetValue(ExtentProperty, value);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size arrangeSize)
    {
        double minX = double.MaxValue;
        double minY = double.MaxValue;

        double maxX = double.MinValue;
        double maxY = double.MinValue;

        var children = Children;
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i] is INodifyCanvasItem item)
            {
                item.Arrange(new Rect(item.Location, item.DesiredSize));

                Size size = children[i].Bounds.Size;

                if (item.Location.X < minX)
                {
                    minX = item.Location.X;
                }

                if (item.Location.Y < minY)
                {
                    minY = item.Location.Y;
                }

                double sizeX = item.Location.X + size.Width;
                if (sizeX > maxX)
                {
                    maxX = sizeX;
                }

                double sizeY = item.Location.Y + size.Height;
                if (sizeY > maxY)
                {
                    maxY = sizeY;
                }
            }
        }

        Extent = minX == double.MaxValue
            ? new Rect(0, 0, 0, 0)
            : new Rect(minX, minY, maxX - minX, maxY - minY);

        return arrangeSize;
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size constraint)
    {
        var availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
        var children = Children;

        for (int i = 0; i < children.Count; i++)
        {
            children[i].Measure(availableSize);
        }

        return default;
    }
}
