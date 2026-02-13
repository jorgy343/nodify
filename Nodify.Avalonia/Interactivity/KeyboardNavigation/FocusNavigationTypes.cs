using Avalonia;

namespace Nodify.Interactivity;

/// <summary>
/// Enum for focus navigation direction - Avalonia equivalent of System.Windows.Input.FocusNavigationDirection
/// </summary>
public enum FocusNavigationDirection
{
    Next,
    Previous,
    First,
    Last,
    Left,
    Right,
    Up,
    Down
}

/// <summary>
/// Represents a request to move focus - Avalonia equivalent of System.Windows.Input.TraversalRequest
/// </summary>
public class TraversalRequest
{
    public FocusNavigationDirection FocusNavigationDirection { get; }
    public bool Wrapped { get; set; }

    public TraversalRequest(FocusNavigationDirection direction)
    {
        FocusNavigationDirection = direction;
    }
}

/// <summary>
/// Extension methods for FocusNavigationDirection
/// </summary>
public static class FocusNavigationDirectionExtensions
{
    public static bool IsOppositeOf(this FocusNavigationDirection direction, FocusNavigationDirection other)
    {
        return (direction == FocusNavigationDirection.Left && other == FocusNavigationDirection.Right)
            || (direction == FocusNavigationDirection.Right && other == FocusNavigationDirection.Left)
            || (direction == FocusNavigationDirection.Up && other == FocusNavigationDirection.Down)
            || (direction == FocusNavigationDirection.Down && other == FocusNavigationDirection.Up)
            || (direction == FocusNavigationDirection.Next && other == FocusNavigationDirection.Previous)
            || (direction == FocusNavigationDirection.Previous && other == FocusNavigationDirection.Next)
            || (direction == FocusNavigationDirection.First && other == FocusNavigationDirection.Last)
            || (direction == FocusNavigationDirection.Last && other == FocusNavigationDirection.First);
    }
}
