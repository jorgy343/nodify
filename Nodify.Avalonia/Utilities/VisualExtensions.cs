using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Linq;

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

    /// <summary>
    /// Gets all visual elements that intersect with the specified geometry.
    /// </summary>
    /// <param name="container">The container to search within.</param>
    /// <param name="geometry">The geometry to test for intersection.</param>
    /// <param name="supportedTypes">The types of elements to include in the results.</param>
    /// <returns>A list of controls that intersect with the geometry.</returns>
    public static List<Control> GetIntersectingElements(this Visual container, Geometry geometry, IReadOnlyCollection<Type> supportedTypes)
    {
        var result = new List<Control>();
        var stack = new Stack<Visual>();
        stack.Push(container);

        while (stack.Count > 0)
        {
            Visual current = stack.Pop();
            var children = current.GetVisualChildren();

            foreach (var child in children)
            {
                if (child is Control control && control.IsHitTestVisible)
                {
                    var controlType = control.GetType();
                    if (supportedTypes.Contains(controlType))
                    {
                        // Check if the control's bounds intersect with the geometry
                        var bounds = control.Bounds;
                        var transform = control.TransformToVisual(container);
                        if (transform != null)
                        {
                            var transformedBounds = bounds.TransformToAABB(transform.Value);
                            var boundsGeometry = new RectangleGeometry(transformedBounds);

                            // Simple intersection test - if the bounds overlap the geometry, consider it intersecting
                            if (DoGeometriesIntersect(geometry, boundsGeometry))
                            {
                                result.Add(control);
                            }
                        }
                    }
                    else
                    {
                        // Continue searching children
                        stack.Push(child);
                    }
                }
                else if (child is Visual visualChild)
                {
                    stack.Push(visualChild);
                }
            }
        }

        return result;
    }

    private static bool DoGeometriesIntersect(Geometry g1, Geometry g2)
    {
        // Simple bounds-based intersection test
        // In a full implementation, you would use more sophisticated geometry intersection
        var bounds1 = g1.Bounds;
        var bounds2 = g2.Bounds;
        return bounds1.Intersects(bounds2);
    }
}
