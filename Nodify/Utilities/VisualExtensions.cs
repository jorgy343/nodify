using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Nodify
{
    internal static class VisualExtensions
    {
        public static T? GetParentOfType<T>(this Visual current)
            where T : Visual
        {
            Visual? parent = current.GetVisualParent();
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
            Visual? parent = current.GetVisualParent();
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

            foreach (var child in visual.GetVisualChildren())
            {
                if (child is T result)
                {
                    return result;
                }

                if (child.GetChildOfType<T>() is T r)
                {
                    return r;
                }
            }

            return default;
        }

        public static T? GetElementAtPosition<T>(this Visual container, Point position)
            where T : Visual
        {
            var hitElement = container.InputHitTest(position) as Visual;
            while (hitElement != null)
            {
                if (hitElement is T result)
                {
                    return result;
                }
                hitElement = hitElement.GetVisualParent();
            }

            return null;
        }

        public static List<Control> GetIntersectingElements(this Visual container, Geometry geometry, IReadOnlyCollection<Type> supportedTypes)
        {
            var result = new List<Control>();
            var geometryBounds = geometry.Bounds;

            var stack = new Stack<Visual>();
            stack.Push(container);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                foreach (var child in current.GetVisualChildren())
                {
                    if (child is Control control && control.IsHitTestVisible)
                    {
                        if (supportedTypes.Contains(control.GetType()))
                        {
                            var topLeft = control.TranslatePoint(new Point(), container) ?? new Point();
                            var bounds = new Rect(topLeft, control.Bounds.Size);
                            if (geometryBounds.Intersects(bounds))
                            {
                                result.Add(control);
                            }
                            // Don't descend into matched elements
                            continue;
                        }
                    }

                    if (child is Visual childVisual)
                    {
                        stack.Push(childVisual);
                    }
                }
            }

            return result;
        }

        public static IEnumerable<T> GetIntersectingElements<T>(this Visual container, Rect area, Func<T, Rect> getBounds)
            where T : Visual
        {
            var stack = new Stack<Visual>();
            stack.Push(container);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                foreach (var child in current.GetVisualChildren())
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

                    if (child is Visual childVisual)
                    {
                        stack.Push(childVisual);
                    }
                }
            }
        }

        #region Animation

        private static readonly Dictionary<(object element, AvaloniaProperty property), CancellationTokenSource> _animations
            = new Dictionary<(object, AvaloniaProperty), CancellationTokenSource>();

        public static void StartAnimation(this Animatable element, StyledProperty<Point> property, Point toValue, double animationDurationSeconds, EventHandler? completedEvent = null)
        {
            CancelAnimation(element, property);

            var fromValue = element.GetValue(property);
            var cts = new CancellationTokenSource();
            _animations[(element, property)] = cts;

            var animation = new Animation
            {
                Duration = TimeSpan.FromSeconds(animationDurationSeconds),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters = { new Setter(property, fromValue) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters = { new Setter(property, toValue) }
                    }
                }
            };

            _ = animation.RunAsync(element, cts.Token).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                {
                    element.SetValue(property, toValue);
                    _animations.Remove((element, property));
                    completedEvent?.Invoke(element, EventArgs.Empty);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static void StartAnimation(this Animatable element, StyledProperty<double> property, double toValue, double animationDurationSeconds, EventHandler? completedEvent = null)
        {
            CancelAnimation(element, property);

            var fromValue = element.GetValue(property);
            var cts = new CancellationTokenSource();
            _animations[(element, property)] = cts;

            var animation = new Animation
            {
                Duration = TimeSpan.FromSeconds(animationDurationSeconds),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters = { new Setter(property, fromValue) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters = { new Setter(property, toValue) }
                    }
                }
            };

            _ = animation.RunAsync(element, cts.Token).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                {
                    element.SetValue(property, toValue);
                    _animations.Remove((element, property));
                    completedEvent?.Invoke(element, EventArgs.Empty);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static void StartLoopingAnimation(this Animatable element, StyledProperty<double> property, double toValue, double durationInSeconds)
        {
            CancelAnimation(element, property);

            var fromValue = element.GetValue(property);
            var cts = new CancellationTokenSource();
            _animations[(element, property)] = cts;

            var animation = new Animation
            {
                Duration = TimeSpan.FromSeconds(durationInSeconds),
                IterationCount = IterationCount.Infinite,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters = { new Setter(property, fromValue) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        Setters = { new Setter(property, toValue) }
                    }
                }
            };

            _ = animation.RunAsync(element, cts.Token);
        }

        public static void CancelAnimation(this Animatable element, AvaloniaProperty property)
        {
            if (_animations.TryGetValue((element, property), out var cts))
            {
                cts.Cancel();
                _animations.Remove((element, property));
            }
        }

        #endregion
    }
}
