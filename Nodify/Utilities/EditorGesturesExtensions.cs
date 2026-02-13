using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Nodify.Interactivity;

namespace Nodify
{
    internal static class EditorGesturesExtensions
    {
        public static SelectionType GetSelectionType(this EditorGestures.SelectionGestures gestures, RoutedEventArgs e)
        {
            if (gestures.Append.Matches(e.Source, e))
            {
                return SelectionType.Append;
            }

            if (gestures.Invert.Matches(e.Source, e))
            {
                return SelectionType.Invert;
            }

            if (gestures.Remove.Matches(e.Source, e))
            {
                return SelectionType.Remove;
            }

            return SelectionType.Replace;
        }

        public static bool TryGetFocusDirection(this EditorGestures.DirectionalNavigationGestures gestures, RoutedEventArgs e, out NavigationDirection direction)
        {
            direction = default;

            if (gestures.Left.Matches(e.Source, e))
            {
                direction = NavigationDirection.Left;
                return true;
            }
            if (gestures.Right.Matches(e.Source, e))
            {
                direction = NavigationDirection.Right;
                return true;
            }
            if (gestures.Up.Matches(e.Source, e))
            {
                direction = NavigationDirection.Up;
                return true;
            }
            if (gestures.Down.Matches(e.Source, e))
            {
                direction = NavigationDirection.Down;
                return true;
            }

            return false;
        }

        public static bool TryGetNavigationDirection(this EditorGestures.DirectionalNavigationGestures gestures, RoutedEventArgs e, out Vector direction)
        {
            double y = gestures.Up.Matches(e.Source, e) ? 1 : gestures.Down.Matches(e.Source, e) ? -1 : 0;
            double x = gestures.Left.Matches(e.Source, e) ? -1 : gestures.Right.Matches(e.Source, e) ? 1 : 0;

            direction = new Vector(x, y);

            return x != 0 || y != 0;
        }

        public static bool IsOppositeOf(this NavigationDirection direction, NavigationDirection other)
        {
            return (direction == NavigationDirection.Left && other == NavigationDirection.Right)
                || (direction == NavigationDirection.Right && other == NavigationDirection.Left)
                || (direction == NavigationDirection.Up && other == NavigationDirection.Down)
                || (direction == NavigationDirection.Down && other == NavigationDirection.Up)
                || (direction == NavigationDirection.Next && other == NavigationDirection.Previous)
                || (direction == NavigationDirection.Previous && other == NavigationDirection.Next)
                || (direction == NavigationDirection.First && other == NavigationDirection.Last)
                || (direction == NavigationDirection.Last && other == NavigationDirection.First);
        }
    }
}
