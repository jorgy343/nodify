using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Controls;
//using Avalonia.Animation; // TODO: Animations need Avalonia-specific implementation

namespace Nodify
{
    internal static class VisualExtensions
    {
        public static T? GetParentOfType<T>(this Visual current)
            where T : class
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

        public static T? GetChildOfType<T>(this Visual? depObj) where T : class
        {
            if (depObj == null)
            {
                return default;
            }

            var children = depObj.GetVisualChildren();
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

        public static T? GetElementAtPosition<T>(this Control container, Point position)
            where T : Control
        {
            var result = container.InputHitTest(position) as T;
            return result;
        }

        public static List<Control> GetIntersectingElements(this Control container, Geometry geometry, IReadOnlyCollection<Type> supportedTypes)
        {
            var result = new List<Control>();
            // TODO: Implement geometry-based hit testing for Avalonia
            // This is a simplified version - full implementation would need custom logic
            return result;
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

        #region Animation
        // TODO: Avalonia uses a different animation system
        // These methods need to be reimplemented using Avalonia.Animation
        // For now, commenting out to allow compilation

        /*
        public static void StartAnimation(this Control animatableElement, AvaloniaProperty dependencyProperty, Point toValue, double animationDurationSeconds, EventHandler? completedEvent = null)
        {
            // TODO: Implement using Avalonia animation system
        }

        public static void StartAnimation(this Control animatableElement, AvaloniaProperty dependencyProperty, double toValue, double animationDurationSeconds, EventHandler? completedEvent = null)
        {
            // TODO: Implement using Avalonia animation system
        }

        public static void StartLoopingAnimation(this Control animatableElement, AvaloniaProperty dependencyProperty, double toValue, double durationInSeconds)
        {
            // TODO: Implement using Avalonia animation system
        }

        public static void CancelAnimation(this Control animatableElement, AvaloniaProperty dependencyProperty)
        {
            // TODO: Implement using Avalonia animation system
        }
        */

        #endregion
    }
}
