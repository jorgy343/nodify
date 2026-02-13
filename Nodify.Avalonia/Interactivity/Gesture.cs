using Avalonia.Interactivity;

namespace Nodify.Interactivity;

/// <summary>
/// Base class for input gestures in the Nodify Interactivity system.
/// This is the Avalonia equivalent of WPF's InputGesture.
/// </summary>
public abstract class Gesture
{
    /// <summary>
    /// Determines whether this gesture matches the specified input event.
    /// </summary>
    /// <param name="targetElement">The target element of the event.</param>
    /// <param name="inputEventArgs">The event arguments.</param>
    /// <returns>True if the gesture matches; otherwise, false.</returns>
    public abstract bool Matches(object? targetElement, RoutedEventArgs inputEventArgs);
}
