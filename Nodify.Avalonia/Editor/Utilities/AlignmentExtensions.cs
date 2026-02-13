using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nodify;

internal static class AlignmentExtensions
{
    public static void Align(this IEnumerable<ItemContainer> values, Alignment alignment, ItemContainer? relativeTo)
    {
        var containers = values as IReadOnlyCollection<ItemContainer> ?? values.ToList();
        switch (alignment)
        {
            case Alignment.Top:
                AlignTop(containers, relativeTo);
                break;

            case Alignment.Left:
                AlignLeft(containers, relativeTo);
                break;

            case Alignment.Bottom:
                AlignBottom(containers, relativeTo);
                break;

            case Alignment.Right:
                AlignRight(containers, relativeTo);
                break;

            case Alignment.Middle:
                AlignMiddle(containers, relativeTo);
                break;

            case Alignment.Center:
                AlignCenter(containers, relativeTo);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
        }
    }

    private static void AlignTop(IReadOnlyCollection<ItemContainer> containers, ItemContainer? instigator)
    {
        double top = instigator?.Location.Y ?? containers.Min(x => x.Location.Y);
        foreach (var c in containers)
        {
            c.Location = new Point(c.Location.X, top);
        }
    }

    private static void AlignLeft(IReadOnlyCollection<ItemContainer> containers, ItemContainer? instigator)
    {
        double left = instigator?.Location.X ?? containers.Min(x => x.Location.X);
        foreach (var c in containers)
        {
            c.Location = new Point(left, c.Location.Y);
        }
    }

    private static void AlignBottom(IReadOnlyCollection<ItemContainer> containers, ItemContainer? instigator)
    {
        double bottom = instigator != null ? instigator.Location.Y + instigator.Bounds.Height : containers.Max(x => x.Location.Y + x.Bounds.Height);
        foreach (var c in containers)
        {
            c.Location = new Point(c.Location.X, bottom - c.Bounds.Height);
        }
    }

    private static void AlignRight(IReadOnlyCollection<ItemContainer> containers, ItemContainer? instigator)
    {
        double right = instigator != null ? instigator.Location.X + instigator.Bounds.Width : containers.Max(x => x.Location.X + x.Bounds.Width);
        foreach (var c in containers)
        {
            c.Location = new Point(right - c.Bounds.Width, c.Location.Y);
        }
    }

    private static void AlignMiddle(IReadOnlyCollection<ItemContainer> containers, ItemContainer? instigator)
    {
        double mid = instigator != null ? instigator.Location.Y + instigator.Bounds.Height / 2 : containers.Average(c => c.Location.Y + c.Bounds.Height / 2);
        foreach (var c in containers)
        {
            c.Location = new Point(c.Location.X, mid - c.Bounds.Height / 2);
        }
    }

    private static void AlignCenter(IReadOnlyCollection<ItemContainer> containers, ItemContainer? instigator)
    {
        double center = instigator != null ? instigator.Location.X + instigator.Bounds.Width / 2 : containers.Average(c => c.Location.X + c.Bounds.Width / 2);
        foreach (var c in containers)
        {
            c.Location = new Point(center - c.Bounds.Width / 2, c.Location.Y);
        }
    }
}
