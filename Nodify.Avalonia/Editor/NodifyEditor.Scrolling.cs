using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;

namespace Nodify;

public partial class NodifyEditor : IScrollable
{
    /// <summary>
    /// The number of units the mouse wheel is rotated to scroll one line.
    /// </summary>
    public static double ScrollIncrement { get; set; } = 15.0;

    private double _extentWidth;
    private double _extentHeight;
    private double _horizontalOffset;
    private double _verticalOffset;
    private Point? _viewportLocationBeforeScrolling;
    private bool _isScrolling;

    // IScrollable implementation for Avalonia
    Size IScrollable.Extent => new Size(_extentWidth, _extentHeight);

    Vector IScrollable.Offset
    {
        get => new Vector(_horizontalOffset, _verticalOffset);
        set
        {
            _horizontalOffset = double.IsInfinity(value.X) ? 0d : value.X;
            _verticalOffset = double.IsInfinity(value.Y) ? 0d : value.Y;
            UpdateViewportLocationOnScroll();
        }
    }

    Size IScrollable.Viewport => ViewportSize;

    private void UpdateViewportLocationOnScroll()
    {
        if (!_viewportLocationBeforeScrolling.HasValue)
        {
            _viewportLocationBeforeScrolling = ViewportLocation;
        }

        _isScrolling = true;

        double locationX = Math.Min(ItemsExtent.Left, _viewportLocationBeforeScrolling.Value.X) + _horizontalOffset;
        double locationY = Math.Min(ItemsExtent.Top, _viewportLocationBeforeScrolling.Value.Y) + _verticalOffset;
        ViewportLocation = new Point(locationX, locationY);

        _isScrolling = false;
    }

    private void UpdateScrollbars()
    {
        // Setting the ViewportLocation when manually scrolling triggers the ViewportUpdatedEvent
        // which in turn calls this method, hence the !_isScrolling check
        if (!_isScrolling)
        {
            _viewportLocationBeforeScrolling = null;

            var extent = ItemsExtent;
            extent = extent.Union(new Rect(ViewportLocation, ViewportSize));

            _extentHeight = extent.Height;
            _extentWidth = extent.Width;

            var scrollOffset = ViewportLocation - ItemsExtent.Position;

            _horizontalOffset = Math.Max(0, scrollOffset.X);
            _verticalOffset = Math.Max(0, scrollOffset.Y);

            // Invalidate measure/arrange to update scroll viewers
            InvalidateMeasure();
        }
    }
}
