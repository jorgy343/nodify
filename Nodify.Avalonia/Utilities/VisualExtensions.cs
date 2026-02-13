using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;

namespace Nodify;

internal static class VisualExtensions
{
    public static T? GetParentOfType<T>(this Visual current)
        where T : Visual
    {
        var parent = current.GetVisualParent();
        while (parent != null)
        {
            if (parent is T match)
            {
                return match;
            }
            parent = parent.GetVisualParent();
        }

        return null;
    }

    public static Visual? GetParent(this Visual current, Func<Visual, bool> condition)
    {
        var parent = current.GetVisualParent();
        while (parent != null)
        {
            if (condition(parent))
            {
                return parent;
            }
            parent = parent.GetVisualParent();
        }

        return null;
    }

    public static T? GetChildOfType<T>(this Visual? visual) where T : Visual
    {
        if (visual == null)
        {
            return default;
        }

        var children = visual.GetVisualChildren();
        foreach (var child in children)
        {
            if (child is T result)
            {
                return result;
            }

            if (GetChildOfType<T>(child) is T r)
            {
                return r;
            }
        }

        return default;
    }

    public static T? GetElementAtPosition<T>(this InputElement container, Point position)
        where T : Visual
    {
        // In Avalonia, use InputHitTest on InputElement
        var inputElement = container.InputHitTest(position);

        if (inputElement is T result)
        {
            return result;
        }

        // Walk up the visual tree to find a parent of type T
        if (inputElement is Visual visual)
        {
            return visual.GetParentOfType<T>();
        }

        return default;
    }

    public static IEnumerable<T> GetIntersectingElements<T>(this Visual container, Rect area, Func<T, Rect> getBounds)
        where T : Visual
    {
        var stack = new Stack<Visual>();
        stack.Push(container);

        while (stack.Count > 0)
        {
            Visual current = stack.Pop();
            var children = current.GetVisualChildren();

            foreach (var child in children)
            {
                if (child is T tChild)
                {
                    var bounds = getBounds(tChild);
                    if (bounds.Intersects(area))
                    {
                        yield return tChild;
                        continue;
                    }
                }

                stack.Push(child);
            }
        }
    }
}
